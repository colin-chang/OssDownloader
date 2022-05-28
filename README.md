# OSS下载器

## 1. OSS 配置

根据实际 OSS 配置更新 `appsettings.json` 即可。

## 2. 下载配置

### 待下载文件

待下载OSS文件`Objects`定义在`src.txt`文件中，每行一个文件。

### 下载目录

下载文件保存路径通过`SAVE_PATH`环境变量进行配置，默认为`./download`目录。

### 最大并行下载任务

最大并行下载数通过`MAX_TASK`环境变量进行配置，默认并行数量为 5 个，可以根据硬件配置自行调整。

## 3. 运行

### 编译运行

运行程序前确保安装了 [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)。

```bash
cd ColinChang.OssDownloader && dotnet run
```

### Docker

#### Docker Hub 镜像
根据实际配置替换以下`待下载文件`、`OSS配置`和`文件保存路径`后运行以下命令即可。
```bash
docker run -it --rm \
-v $PWD/src.txt:/app/src.txt \
-v $PWD/appsettings.json:/app/appsettings.json \
-v $PWD/download:/app/download \
-e MAX_TASK=5 \
colinchang/ossdownloader
```

#### 自编译镜像

```bash
# 编译镜像
cd ColinChang.OssDownloader && rm -rf ../Docker/publish && dotnet publish -c Release -o ../Docker/publish
cd ../Docker && docker build -t colinchang/ossdownloader .

# 运行容器
docker run -it --rm -v /Users/Colin/Downloads:/app/download colinchang/ossdownloader
```