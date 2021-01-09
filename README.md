# 运行说明

**特别注意：BVH数据一定要有Tpose，一般要么是内置骨骼为Tpose，要么是内置为Apos而第一帧为Tpose**

- 创建角色 随便找个avatar角色或者使用工程中提供的`Assets/Models`中的两个角色(`unity-chan`或`Rin`)
- 挂载脚本 将`Scripts/BVHDriver.cs`拖拽到创建的角色上
- 脚本设置
  - `Bonemaps`：设置关节对应关系 脚本的`Bonemaps`定义了`unity`自己的`humanoid`的关节与`BVH`数据关节的对应关系，最好按照骨骼的层级关系填写
  - `FirstT`：注意你的BVH文件第一帧是Tpos，或者说内置的skeleton就是T-pose，可以通过`bvhacker`查看（**如CMU提供的BVH数据内置骨骼是A-pos，第一帧是Tpose，所以需要勾选；示例中的tmp.bvh的内置骨骼的Tpos，所以无需勾选**）
  - `TargetAvatar`：创建的角色
  - `filename`：`bvh`的路径

# 强调

代码中需要注意bvh可视化的结果对不对，如果不对，说明全局旋转没计算对，因为我按照`ZYX`的欧拉角顺序计算的全局旋转，如果是其它顺序，请重写一下`BVHParser.cs`里面的`eul2quat`函数。

还有可能由于模型缩放，unity角色的动作没问题但是位置有问题，那就需要调整`scaleRatio`了，请自行尝试。