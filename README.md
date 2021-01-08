# 运行说明

**特别注意：BVH数据一定要有Tpose，一般要么是内置骨骼为Tpose，要么是内置为Apos而第一帧为Tpose**

- 创建角色
  随便找个avatar角色或者使用工程中提供的`Assets/Models`中的两个角色(`unity-chan`或`Rin`)
- 挂载脚本
  将`Scripts/BVHDriver.cs`拖拽到创建的角色上
- 设置关节对应关系
  脚本的`Bonemaps`定义了`unity`自己的`humanoid`的关节与`BVH`数据关节的对应关系，最好按照骨骼的层级关系填写
- `FirstT`：注意你的BVH文件第一帧是Tpos，或者说内置的skeleton就是T-pose，可以通过`bvhacker`查看（**如CMU提供的BVH数据内置骨骼是A-pos，第一帧是Tpose，所以需要勾选；示例中的tmp.bvh的内置骨骼的Tpos，所以无需勾选**）
- 其它需要拖拽的设置
  - `targetAvatar`：是创建的角色
  - `BVH Path`：是输入BVH文件路径的文本框(`Hierarchy->UI->BVH->BVHPath`)
  - `BVH Log`：显示BVH路径是否存在(`Hierarchy->UI->BVH->BVHExist`)
  - `Audio Path`：是输入音频文件路径的文本框(`Hierarchy->UI->Audio->AudioPath`)
  - `Audio Log`：显示音频文件是否存在(`Hierarchy->UI->Audio->AudioExist`)
  - `Audio Source`：播放音频的组件(`Hierarchy->UI->Audio->Audio Source`)
  - `PlayBtn`：加载BVH和音频完毕以后，播放动画和音频的按钮(`Hierarchy->UI->FunctionButton->Play`)
  - `UI Canva`：播放的时候隐藏界面按钮和输入框之类的

