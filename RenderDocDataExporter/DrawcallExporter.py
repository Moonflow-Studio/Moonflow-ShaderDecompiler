# The tool based on renderdoc.lib and qrenderdoc.lib

import os, sys, struct, subprocess, datetime
from pathlib import Path
import numpy as np
from numba import njit

#######################################################################################################################
#   Config Part  ######################################################################################################
#######################################################################################################################

filename = "E:/Intime/FrameCapture/AFK/MuMuVMMHeadless_2024.11.13_15.35_frame2730.rdc"
target_folder = "C:/Users/frankmhli/Documents/Project/Moonflow/TempMF/Assets/moonflow-system/Tools/Editor/MFUtilityTools/MFShaderDecompiler/RenderDocDataExporter/Release"
spirv_cross_path = "C:/Program Files/RenderDoc/plugins/spirv/spirv-cross.exe"

# Because of read mesh data cost too much time, if you don't need to refresh extracted mesh data, set it to False to skip the process
read_mesh_data = True

startEventID = 806
endEventID = 1727

#######################################################################################################################
#  Before Sample ######################################################################################################
#######################################################################################################################

# Import renderdoc if not already imported (e.g. in the UI)
if 'renderdoc' not in sys.modules and '_renderdoc' not in sys.modules:
    import renderdoc

if 'qrenderdoc' not in sys.modules and '_qrenderdoc' not in sys.modules:
    import qrenderdoc



# Alias renderdoc for legibility
rd = renderdoc
qrd = qrenderdoc
rd.InitialiseReplay(rd.GlobalEnvironment(), [])

# We base our data on a MeshFormat, but we add some properties
class MeshData(rd.MeshFormat):
    indexOffset = 0
    name = ''

# Unpack a tuple of the given format, from the data
def unpackData(fmt, data):
    # We don't handle 'special' formats - typically bit-packed such as 10:10:10:2
    if fmt.Special():
        raise RuntimeError("Packed formats are not supported!")

    formatChars = {}
    #                                 012345678
    formatChars[rd.CompType.UInt] = "xBHxIxxxL"
    formatChars[rd.CompType.SInt] = "xbhxixxxl"
    formatChars[rd.CompType.Float] = "xxexfxxxd"  # only 2, 4 and 8 are valid

    # These types have identical decodes, but we might post-process them
    formatChars[rd.CompType.UNorm] = formatChars[rd.CompType.UInt]
    formatChars[rd.CompType.UScaled] = formatChars[rd.CompType.UInt]
    formatChars[rd.CompType.SNorm] = formatChars[rd.CompType.SInt]
    formatChars[rd.CompType.SScaled] = formatChars[rd.CompType.SInt]

    # We need to fetch compCount components
    vertexFormat = str(fmt.compCount) + formatChars[fmt.compType][fmt.compByteWidth]

    # Unpack the data
    value = struct.unpack_from(vertexFormat, data, 0)

    # If the format needs post-processing such as normalisation, do that now
    if fmt.compType == rd.CompType.UNorm:
        divisor = float((2 ** (fmt.compByteWidth * 8)) - 1)
        value = tuple(float(i) / divisor for i in value)
    elif fmt.compType == rd.CompType.SNorm:
        maxNeg = -float(2 ** (fmt.compByteWidth * 8)) / 2
        divisor = float(-(maxNeg - 1))
        value = tuple((float(i) if (i == maxNeg) else (float(i) / divisor)) for i in value)

    # If the format is BGRA, swap the two components
    if fmt.BGRAOrder():
        value = tuple(value[i] for i in [2, 1, 0, 3])

    return value


# Get a list of MeshData objects describing the vertex inputs at this draw
def getMeshInputs(controller, draw):
    state = controller.GetPipelineState()

    # Get the index & vertex buffers, and fixed vertex inputs
    ib = state.GetIBuffer()
    vbs = state.GetVBuffers()
    attrs = state.GetVertexInputs()

    meshInputs = []

    for attr in attrs:

        # We don't handle instance attributes
        if attr.perInstance:
            raise RuntimeError("Instanced properties are not supported!")

        meshInput = MeshData()
        meshInput.indexResourceId = ib.resourceId
        meshInput.indexByteOffset = ib.byteOffset
        meshInput.indexByteStride = ib.byteStride
        meshInput.baseVertex = draw.baseVertex
        meshInput.indexOffset = draw.indexOffset
        meshInput.numIndices = draw.numIndices

        # If the draw doesn't use an index buffer, don't use it even if bound
        if not (draw.flags & rd.ActionFlags.Indexed):
            meshInput.indexResourceId = rd.ResourceId.Null()

        # The total offset is the attribute offset from the base of the vertex
        meshInput.vertexByteOffset = attr.byteOffset + vbs[attr.vertexBuffer].byteOffset + draw.vertexOffset * vbs[
            attr.vertexBuffer].byteStride
        meshInput.format = attr.format
        meshInput.vertexResourceId = vbs[attr.vertexBuffer].resourceId
        meshInput.vertexByteStride = vbs[attr.vertexBuffer].byteStride
        meshInput.name = attr.name

        meshInputs.append(meshInput)

    return meshInputs


# Get a list of MeshData objects describing the vertex outputs at this draw
def getMeshOutputs(controller, postvs):
    meshOutputs = []
    posidx = 0

    vs = controller.GetPipelineState().GetShaderReflection(rd.ShaderStage.Vertex)

    # Repeat the process, but this time sourcing the data from postvs.
    # Since these are outputs, we iterate over the list of outputs from the
    # vertex shader's reflection data
    for attr in vs.outputSignature:
        # Copy most properties from the postvs struct
        meshOutput = MeshData()
        meshOutput.indexResourceId = postvs.indexResourceId
        meshOutput.indexByteOffset = postvs.indexByteOffset
        meshOutput.indexByteStride = postvs.indexByteStride
        meshOutput.baseVertex = postvs.baseVertex
        meshOutput.indexOffset = 0
        meshOutput.numIndices = postvs.numIndices

        # The total offset is the attribute offset from the base of the vertex,
        # as calculated by the stride per index
        meshOutput.vertexByteOffset = postvs.vertexByteOffset
        meshOutput.vertexResourceId = postvs.vertexResourceId
        meshOutput.vertexByteStride = postvs.vertexByteStride

        # Construct a resource format for this element
        meshOutput.format = rd.ResourceFormat()
        meshOutput.format.compByteWidth = rd.VarTypeByteSize(attr.varType)
        meshOutput.format.compCount = attr.compCount
        meshOutput.format.compType = rd.VarTypeCompType(attr.varType)
        meshOutput.format.type = rd.ResourceFormatType.Regular

        meshOutput.name = attr.semanticIdxName if attr.varName == '' else attr.varName

        if attr.systemValue == rd.ShaderBuiltin.Position:
            posidx = len(meshOutputs)

        meshOutputs.append(meshOutput)

    # Shuffle the position element to the front
    if posidx > 0:
        pos = meshOutputs[posidx]
        del meshOutputs[posidx]
        meshOutputs.insert(0, pos)

    accumOffset = 0

    for i in range(0, len(meshOutputs)):
        meshOutputs[i].vertexByteOffset = accumOffset

        # Note that some APIs such as Vulkan will pad the size of the attribute here
        # while others will tightly pack
        fmt = meshOutputs[i].format

        accumOffset += (8 if fmt.compByteWidth > 4 else 4) * fmt.compCount

    return meshOutputs


def getIndices(controller, mesh):
    # Get the character for the width of index
    indexFormat = 'B'
    if mesh.indexByteStride == 2:
        indexFormat = 'H'
    elif mesh.indexByteStride == 4:
        indexFormat = 'I'

    # Duplicate the format by the number of indices
    indexFormat = str(mesh.numIndices) + indexFormat

    # If we have an index buffer
    if mesh.indexResourceId != rd.ResourceId.Null():
        # Fetch the data
        ibdata = controller.GetBufferData(mesh.indexResourceId, mesh.indexByteOffset, 0)

        # Unpack all the indices, starting from the first index to fetch
        offset = mesh.indexOffset * mesh.indexByteStride
        indices = struct.unpack_from(indexFormat, ibdata, offset)

        # Apply the baseVertex offset
        return [i + mesh.baseVertex for i in indices]
    else:
        # With no index buffer, just generate a range
        return tuple(range(mesh.numIndices))


def extract_mesh_input(controller, meshData, eventId):
    indices = getIndices(controller, meshData[0])
    text = 'VertexIndex'
    for attr in meshData:
        text += ' ' + str(attr.name)
    text += '\n\n'

    length = len(indices)
    # We'll decode the first three indices making up a triangle
    vdict = dict()
    viarray = []
    index = 0
    print("Combing through %d indices" % length)
    inlinetext_parts = []
    for i in indices:
        idx = indices[index]
        viarray.append(idx)
        print("Index %d / %d" % (index, length))
        # inlinetext = str(idx)
        inlinetext_parts.append(str(idx))
        for attr in meshData:
            # This is the data we're reading from. This would be good to cache instead of
            # re-fetching for every attribute for every index
            offset = attr.vertexByteOffset + attr.vertexByteStride * idx
            data = controller.GetBufferData(attr.vertexResourceId, offset, 0)

            # Get the value from the data
            value = unpackData(attr.format, data)

            # We don't go into the details of semantic matching here, just print both
            # print("\tAttribute '%s': %s" % (attr.name, value))
            # inlinetext += ' ' + str(value)
            inlinetext_parts.append(str(value))
        # vdict[idx] = inlinetext + '\n'
        vdict[idx] = ' '.join(inlinetext_parts) + '\n'
        index = index + 1
        inlinetext_parts.clear()
    print("Writing %d vertices" % length)
    text += ''.join(vdict.values())

    print("Writing %d indices" % length)
    indices_str = '\n'.join(map(str, viarray))

    print("Writing to disk")
    path = Path(target_folder + '\\' + str(eventId) + '\\' + 'VertexIndices.txt')
    path.write_text(indices_str)
    path = Path(target_folder + '\\' + str(eventId) + '\\' + 'VertexInputData.txt')
    path.write_text(text)


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


def translate_spirv(stage_name, shader_source_path, evt_id, stage_tag, entry_point, shader_res_id):
    output_file_name = target_folder + '/' + str(evt_id) + '/' + stage_name + "_" + shader_res_id + "_output_hlsl.txt"
    # command = [spirv_cross_path, shader_source_path]
    command = [spirv_cross_path, shader_source_path, "--output", output_file_name, "--hlsl", "--entry", entry_point, "--stage", stage_tag]
    # command = [glslang_validator_path, "-H", "-V", "-o", shader_source_path, output_file_name]
    # command = ["glslangValidator", "-H", "-V", "-o", shader_source_path, output_file_name, "./spirv-cross", "--version 450", "--es", shader_source_path]
    try:
        result = subprocess.run(command, shell=True)
        if result.returncode == 0:
            # print("转译成功，输出：", result.stdout)
            # 可以进一步处理转译后的文件，如读取内容等
            with open(output_file_name, "r") as output_file:
                output_content = output_file.read()
                # print("转译后的内容：", output_content)
        else:
            print("转译失败，错误输出：", str(result.returncode))
    except FileNotFoundError:
        print("SPIRV - Cross.exe或shader源码文件未找到")

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


def variable_to_text(sv, indent):
    varstr = indent
    if len(sv.members) == 0:
        varstr += str(sv.name) + '  '
        inlineindex = 0
        while inlineindex < sv.columns:
            if sv.type is rd.VarType.Float:
                varstr += str(sv.value.f32v[inlineindex]) + ' '
                # print(str(sv.name)+str(sv.value.f32v[inlineindex]))
            elif sv.type is rd.VarType.Half:
                varstr += str(sv.value.f16v[inlineindex]) + ' '
                # print(str(sv.name)+str(sv.value.f16v[inlineindex]))
            elif sv.type is rd.VarType.Double:
                varstr += str(sv.value.f64v[inlineindex]) + ' '
            elif sv.type is rd.VarType.SShort:
                varstr += str(sv.value.s16v[inlineindex]) + ' '
            elif sv.type is rd.VarType.SInt:
                varstr += str(sv.value.s32v[inlineindex]) + ' '
            elif sv.type is rd.VarType.SLong:
                varstr += str(sv.value.s64v[inlineindex]) + ' '
            elif sv.type is rd.VarType.UShort:
                varstr += str(sv.value.u16v[inlineindex]) + ' '
            elif sv.type is rd.VarType.UInt:
                varstr += str(sv.value.u32v[inlineindex]) + ' '
            elif sv.type is rd.VarType.ULong:
                varstr += str(sv.value.u64v[inlineindex]) + ' '
            elif sv.type is rd.VarType.SByte:
                varstr += str(sv.value.s8v[inlineindex]) + ' '
            elif sv.type is rd.VarType.UByte:
                varstr += str(sv.value.u8v[inlineindex]) + ' '
            inlineindex = inlineindex + 1
        varstr += '\n'
    else:
        varstr += str(sv.name) + '  MEMBERS:' + str(len(sv.members)) + '\n'
        for m in sv.members:
            varstr += variable_to_text(m, indent + '    ')
    return varstr


def cbuffer_list_to_text(cblist):
    indent = ''
    varstr = ''
    for sv in cblist:
        varstr += variable_to_text(sv, '')
    return varstr


def disassemble_cbuffers(controller, refl, state, stage, pipeline, eventId):
    blocks_data = state.GetConstantBlocks(stage, True)
    block_count = len(blocks_data)
    print(str(stage)+'  CBuffers length: ' + str(block_count))
    entry = state.GetShaderEntryPoint(stage)
    cbuffer_list = []
    const_blocks = refl.constantBlocks
    buffer_index = 0
    stage_name = str(stage).replace("ShaderStage.", '')
    while buffer_index < block_count:
        resource_name = str(blocks_data[buffer_index].descriptor.resource).replace('ResourceId::', '')
        print("Load Stage: " + stage_name + "  CBuffer: " + str(buffer_index) + "  Name: " + str(blocks_data[buffer_index].descriptor.resource) + "  Shader:" + str(refl.resourceId))
        cbuffer_variables = controller.GetCBufferVariableContents(pipeline, refl.resourceId, stage, entry, buffer_index,
                                                                  blocks_data[buffer_index].descriptor.resource, blocks_data[buffer_index].descriptor.byteOffset, blocks_data[buffer_index].descriptor.byteSize)
        if len(cbuffer_variables) == 20:
            new_file_name = 'CBuffer_' + stage_name + '_s' + str(const_blocks[buffer_index].fixedBindSetOrSpace) + '_b' + str(const_blocks[buffer_index].fixedBindNumber) + '_' + str(const_blocks[buffer_index].name) + '_o' + str(blocks_data[buffer_index].descriptor.byteOffset) + '_' + resource_name + '_UnityPerDraw'
        else:
            new_file_name = 'CBuffer_' + stage_name + '_s' + str(const_blocks[buffer_index].fixedBindSetOrSpace) + '_b' + str(const_blocks[buffer_index].fixedBindNumber) + '_' + str(const_blocks[buffer_index].name) + '_o' + str(blocks_data[buffer_index].descriptor.byteOffset) + '_' + resource_name + '_Unknown'
        # if len(cbuffer_variables):
        #     new_file_name = 'CBuffer_' + stage_name + '_' + str(buffer_index) + '_' + resource_name + '_UnityPerMaterial'
        # else:
        #     new_file_name = 'CBuffer_' + stage_name + '_' + str(buffer_index) + '_' + resource_name + '_Unknown'
        text_path = Path(target_folder + '\\' + str(eventId) + '\\' + new_file_name + '.txt')
        cbuffer_list.append(cbuffer_variables)
        text_path.write_text(cbuffer_list_to_text(cbuffer_variables))
        buffer_index = buffer_index + 1

    index = 0


def disassemble_textures(ctrl, refl, state, stage, path):
    ress = refl.readOnlyResources
    sress = state.GetReadOnlyResources(stage, True)
    stage_name = str(stage).replace("ShaderStage.", '')
    if len(ress) == len(sress):
        index = 0
        while index < len(ress):
            if sress[index].descriptor.type == rd.DescriptorType.ImageSampler:
                resName = stage_name + '_s' + str(ress[index].fixedBindSetOrSpace) + '_b' + str(
                    ress[index].fixedBindNumber) + '_' + str(ress[index].name) + '_' + str(
                    sress[index].descriptor.resource).replace('ResourceId::', '')
                texsave = rd.TextureSave()
                texsave.resourceId = sress[index].descriptor.resource
                if texsave.resourceId == rd.ResourceId.Null():
                    continue
                texsave.alpha = rd.AlphaMapping.BlendToCheckerboard
                texsave.mip = 0
                texsave.slice.sliceIndex = 0
                texsave.destType = rd.FileType.PNG
                realPath = path + resName
                ctrl.SaveTexture(texsave, realPath + ".png")
            index = index + 1


def disassemble_vertex_shader(ctrl, state, pipeline, target, evt_id):
    print("disassemble_vertex_shader")
    shader_refl_vert = state.GetShaderReflection(rd.ShaderStage.Vertex)
    vertex_shader_text = ctrl.DisassembleShader(pipeline, shader_refl_vert, target)
    shader_res_id = str(shader_refl_vert.resourceId).replace('ResourceId::', '')
    text_path_name = target_folder + '/' + str(evt_id) + '/' + 'vs_original_'+shader_res_id+'.txt'
    text_path = Path(text_path_name)
    text_path.write_text(vertex_shader_text)

    entryPoint_vert = state.GetShaderEntryPoint(rd.ShaderStage.Vertex)
    shader_resId = shader_refl_vert.resourceId

    disassemble_cbuffers(ctrl, shader_refl_vert, state, rd.ShaderStage.Vertex, pipeline, evt_id)
    disassemble_textures(ctrl, shader_refl_vert, state, rd.ShaderStage.Vertex, target_folder + '\\' + str(evt_id) + '\\')

    byte_path_name = target_folder + '/' + str(evt_id) + '/' + 'vs_original' + shader_res_id + '.spv'
    byte_path = Path(byte_path_name)
    byte_path.write_bytes(shader_refl_vert.rawBytes)
    translate_spirv('vs', byte_path_name, evt_id, 'vert', str(shader_refl_vert.entryPoint), shader_res_id)

def disassemble_pixel_shader(ctrl, state, pipeline, target, evt_id):
    print("disassemble_pixel_shader")
    shader_refl_pixel = state.GetShaderReflection(rd.ShaderStage.Pixel)
    pixel_shader_text = ctrl.DisassembleShader(pipeline, shader_refl_pixel, target)
    shader_res_id = str(shader_refl_pixel.resourceId).replace('ResourceId::', '')
    text_path_name = target_folder + '\\' + str(evt_id) + '\\' + 'ps_original_' + shader_res_id + '.txt'
    text_path = Path(text_path_name)
    text_path.write_text(pixel_shader_text)

    entryPoint_pixel = state.GetShaderEntryPoint(rd.ShaderStage.Pixel)
    shader_resId = shader_refl_pixel.resourceId

    disassemble_cbuffers(ctrl, shader_refl_pixel, state, rd.ShaderStage.Pixel, pipeline, evt_id)
    disassemble_textures(ctrl, shader_refl_pixel, state, rd.ShaderStage.Pixel, target_folder + '\\' + str(evt_id) + '\\')

    byte_path_name = target_folder + '/' + str(evt_id) + '/' + 'ps_original.spv'
    byte_path = Path(byte_path_name)
    byte_path.write_bytes(shader_refl_pixel.rawBytes)
    translate_spirv('ps', byte_path_name, evt_id, 'frag', str(shader_refl_pixel.entryPoint), shader_res_id)



def disassemble_shader(ctrl, evt_id):
    targets = ctrl.GetDisassemblyTargets(True)
    print("Allowed disassembly targets:")
    for t in targets:
        print(t)

    target = targets[0]
    state = ctrl.GetPipelineState()
    pipeline = state.GetGraphicsPipelineObject()

    disassemble_vertex_shader(ctrl, state, pipeline, target, evt_id)
    disassemble_pixel_shader(ctrl, state, pipeline, target, evt_id)


def extract_drawcall_data(ctrl, evt_id):
    disassemble_shader(ctrl, evt_id)
    return


def sample_drawcall(drawcall, index, ctrl):
    print('\n\n\n\n')
    if drawcall.eventId < startEventID or drawcall.eventId > endEventID:
        return
    while len(drawcall.children) > 0:
        drawcall = drawcall.children[0]
    print("Action #%d Pass #0 starts with %d: %s" % (
        index, drawcall.eventId, drawcall.GetName(ctrl.GetStructuredFile())))

    if drawcall != None:
        print("Decoding Drawcall Data")
        drawcall_path = Path(target_folder + '\\' + str(drawcall.eventId))
        drawcall_path.mkdir(parents=True, exist_ok=True)
        ctrl.SetFrameEvent(drawcall.eventId, True)
        extract_drawcall_data(ctrl, drawcall.eventId)

        if read_mesh_data:
            meshInputs = getMeshInputs(ctrl, drawcall)
            extract_mesh_input(ctrl, meshInputs, drawcall.eventId)
            current_datetime = datetime.datetime.now()
            formatted_time = current_datetime.strftime("%Y-%m-%d %H-%M-%S")
            print("当前时间: %s" % formatted_time)
            # print("Decoding mesh outputs\n\n")
            # Fetch the postvs data
            # postvs = ctrl.GetPostVSData(0, 0, rd.MeshDataStage.VSOut)
            # Calcualte the mesh configuration from that
            # meshOutputs = getMeshOutputs(ctrl, postvs)
            # Print it
            # extract_mesh_input(ctrl, meshOutputs)

        return


def sampleCode(controller):
    # Iterate over all of the root actions
    for d in controller.GetRootActions():
        iterAction(d, controller)

    # Start iterating from the first real action as a child of markers
    actions = controller.GetRootActions()
    index = 0
    for action in actions:
        sample_drawcall(action, index, controller)
        index = index + 1


# Load capture data
cap, controller = loadCapture(filename)
# Sample capture and do function
sampleCode(controller)
# Shutdown everything
controller.Shutdown()
cap.Shutdown()
rd.ShutdownReplay()
