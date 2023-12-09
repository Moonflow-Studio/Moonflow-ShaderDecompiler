# Readme

### DXBC -> Unity URP typed Shaderlab+HLSL only

（更新中）

__需要安装Package  - Moonflow Core ： https://gitee.com/reguluz/moonflow-core.git__

使用RenderDoc内解析的DXBC作为源码，通过文本识别的方式重新组织运算符与函数的显示方式，重新建立数据的链接以加速逆向Shader的过程。

### 注意事项
由于目前文本识别写法的问题，需要把所有if分支内的语句前的缩进全部去除，且头部定义寄存器的语句前缩进不能动。否则将会识别文本失败，待优化

### 还原前样例

![](https://raw.githubusercontent.com/Reguluz/ImageBed/master/QQ%E6%88%AA%E5%9B%BE20221023164959.png)

### 还原后样例（对应右侧）

![](https://raw.githubusercontent.com/Reguluz/ImageBed/master/QQ%E6%88%AA%E5%9B%BE20221023165256.png)

