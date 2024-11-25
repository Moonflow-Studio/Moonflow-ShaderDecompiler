import os, sys
from pathlib import Path

filename = "E:\Intime\FrameCapture\AFK\MuMuVMMHeadless_2024.11.13_15.35_frame2730.rdc"
target_folder = "E:\\Intime\\FrameCapture\\AFK\\Release"


# Import renderdoc if not already imported (e.g. in the UI)
if 'renderdoc' not in sys.modules and '_renderdoc' not in sys.modules:
    import renderdoc

if 'qrenderdoc' not in sys.modules and '_qrenderdoc' not in sys.modules:
    import qrenderdoc


# Alias renderdoc for legibility
rd = renderdoc
qrd = qrenderdoc
rd.InitialiseReplay(rd.GlobalEnvironment(), [])

startEventID = 816
endEventID = 817


def loadCapture(filename):
    # Open a capture file handle
    cap = rd.OpenCaptureFile()

    # Open a particular file - see also OpenBuffer to load from memory
    result = cap.OpenFile(filename, '', None)

    # Make sure the file opened successfully
    if result != rd.ResultCode.Succeeded:
        raise RuntimeError("Couldn't open file: " + str(result))

    # Make sure we can replay
    if not cap.LocalReplaySupport():
        raise RuntimeError("Capture cannot be replayed")

    # Initialise the replay
    result, controller = cap.OpenCapture(rd.ReplayOptions(), None)

    #if result != rd.ReplayStatus.Succeeded:
    #raise RuntimeError("Couldn't initialise replay: " + str(result))
    return cap, controller


#######################################################################################################################
#  Before Sample ######################################################################################################
#######################################################################################################################

#######################################################################################################################
#   Start Sample ######################################################################################################
#######################################################################################################################
# Define a recursive function for iterating over actions
def iterAction(d, controller, indent=''):
    # Print this action
    # print('%s%d: %s' % (indent, d.eventId, d.GetName(controller.GetStructuredFile())))

    # Iterate over the action's children
    for d in d.children:
        iterAction(d, controller, indent + '    ')


def debug_print_cbuffer(variable_list):
    print(len(variable_list))


def variable_to_text(sv, indent):
    varstr = indent
    if len(sv.members) == 0:
        print(str(sv.type) + '  ' + str(sv.name) + '  Columns:' + str(sv.columns) + '  Rows:' + str(sv.rows))
        varstr += str(sv.name) + '  '
        inlineindex = 0
        if '[' in sv.name:
            print(str(sv.name)+'  insidefloat:'+str(sv.value.f32v))
        while inlineindex < sv.columns:
            if sv.type is rd.VarType.Float:
                varstr += str(sv.value.f32v[inlineindex]) + ' '
            elif sv.type is rd.VarType.Half:
                varstr += str(sv.value.f16v[inlineindex]) + ' '
            inlineindex = inlineindex + 1
        varstr += '\n'
    else:
        varstr += str(sv.name) + '  MEMBERS\n'
        for m in sv.members:
            varstr += variable_to_text(m, indent + '    ')

    return varstr


def cbuffer_list_to_text(cblist):
    indent = ''
    varstr = ''
    for sv in cblist:
        varstr += variable_to_text(sv,'')
    return varstr


def disassemble_cbuffers(controller, refl, state, stage, pipeline, eventId):
    blocks = state.GetConstantBlocks(stage, False)
    # for b in blocks:
        # print("Used block resource Id:%d type%s" % (b.descriptor.resource, b.descriptor.type))
    block_count = len(blocks)

    entry = state.GetShaderEntryPoint(stage)
    cbuffer_list = []
    buffer_index = 0
    stage_name = str(stage).replace("ShaderStage.",'')
    while buffer_index < block_count:
        resource_name = str(blocks[buffer_index].descriptor.resource).replace('ResourceId::','')
        if buffer_index == 1:
            new_file_name = 'CBuffer_' + stage_name + '_' + str(buffer_index) + '_' + resource_name + '_UnityPerDraw'
        elif buffer_index == 2:
            new_file_name = 'CBuffer_' + stage_name + '_' + str(buffer_index) + '_' + resource_name + '_UnityPerMaterial'
        else:
            new_file_name = 'CBuffer_' + stage_name + '_' + str(buffer_index) + '_' + resource_name + '_Unknown'
        text_path = Path(target_folder + '\\' + str(eventId) + '\\' + new_file_name + '.txt')
        cbuffer_variables = controller.GetCBufferVariableContents(pipeline, refl.resourceId, stage, entry, buffer_index, blocks[buffer_index].descriptor.resource,0,0)
        if buffer_index == 0:
            debug_print_cbuffer(cbuffer_variables)
        cbuffer_list.append(cbuffer_variables)
        text_path.write_text(cbuffer_list_to_text(cbuffer_variables))
        buffer_index = buffer_index + 1

    const_blocks = refl.constantBlocks
    index = 0
    for block in const_blocks:
        variables = block.variables
        # for var in variables:
            # print("block %d variable: %s" % (index, var.name))
        index = index + 1


def disassemble_vertex_shader(ctrl, state, pipeline, target, evt_id):
    print("disassemble_vertex_shader")
    shader_refl = state.GetShaderReflection(rd.ShaderStage.Vertex)
    vertex_shader_text = ctrl.DisassembleShader(pipeline, shader_refl, target)
    text_path = Path(target_folder + '\\' + str(evt_id) + '\\' + 'vs.txt')
    text_path.write_text(vertex_shader_text)

    entryPoint = state.GetShaderEntryPoint(rd.ShaderStage.Vertex)
    shader_resId = shader_refl.resourceId

    disassemble_cbuffers(ctrl, shader_refl, state, rd.ShaderStage.Vertex, pipeline, evt_id)


def disassemble_pixel_shader(ctrl, state, pipeline, target, eventId):
    shader_refl = state.GetShaderReflection(rd.ShaderStage.Pixel)
    pixel_shader_text = ctrl.DisassembleShader(pipeline, shader_refl, target)
    text_path = Path(target_folder + '\\' + str(eventId) + '\\' + 'ps.txt')
    text_path.write_text(pixel_shader_text)

    entryPoint = state.GetShaderEntryPoint(rd.ShaderStage.Vertex)
    shader_resId = shader_refl.resourceId

    disassemble_cbuffers(ctrl, shader_refl, state, rd.ShaderStage.Pixel, pipeline, eventId)


def disassemble_shader(ctrl, eventId):
    targets = ctrl.GetDisassemblyTargets(True)
    print("Allowed disassembly targets:")
    for t in targets:
        print(t)

    target = targets[0]
    state = ctrl.GetPipelineState()
    pipeline = state.GetGraphicsPipelineObject()

    disassemble_vertex_shader(ctrl, state, pipeline, target, eventId)
    disassemble_pixel_shader(ctrl, state, pipeline, target, eventId)


def extract_drawcall_data(ctrl, eventId):
    disassemble_shader(ctrl, eventId)
    return


def sample_drawcall(drawcall, index):
    if drawcall.eventId < startEventID or drawcall.eventId > endEventID:
        return
    while len(drawcall.children) > 0:
        drawcall = drawcall.children[0]
    print(" ")
    print(" ")
    print(" ")
    print("Action #%d Pass #0 starts with %d: %s" % (
    index, drawcall.eventId, drawcall.GetName(controller.GetStructuredFile())))

    if drawcall != None:
        drawcall_path = Path(target_folder + '\\' + str(drawcall.eventId))
        drawcall_path.mkdir(parents=True, exist_ok=True)
        controller.SetFrameEvent(drawcall.eventId, True)
        extract_drawcall_data(controller, drawcall.eventId)
        return


def sampleCode(controller):
    # Iterate over all of the root actions
    for d in controller.GetRootActions():
        iterAction(d, controller)

    # Start iterating from the first real action as a child of markers
    actions = controller.GetRootActions()
    index = 0
    for action in actions:
        sample_drawcall(action, index)
        index = index + 1


# Load capture data
cap, controller = loadCapture(filename)
# Sample capture and do function
sampleCode(controller)
# Shutdown everything
controller.Shutdown()
cap.Shutdown()
rd.ShutdownReplay()
