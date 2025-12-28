# 配置敏感信息的方法

## 开发环境 - User Secrets（推荐）

已为项目启用 User Secrets，敏感信息存储在本地用户目录，不会提交到 Git。

### 查看已保存的密钥
```bash
dotnet user-secrets list
```

### 添加新密钥
```bash
dotnet user-secrets set "配置:键名" "值"
```

### 删除密钥
```bash
dotnet user-secrets remove "配置:键名"
```

### 密钥存储位置
- Windows: `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`
- macOS/Linux: `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`

User Secrets 会自动覆盖 appsettings.json 中的相应配置。

## 备选方案

### 1. 本地配置文件（已配置）
创建 `appsettings.local.json`（已添加到 .gitignore）：
- 复制 `appsettings.local.example.json` 为 `appsettings.local.json`
- 填入真实的密钥值
- 该文件会被 Git 忽略，不会提交

### 2. 环境变量
在系统或 IDE 中设置环境变量：
```
Ai__ApiKey=your-key
Oss__AccessKeyId=your-key
Oss__AccessKeySecret=your-secret
```

### 3. 生产环境
- 使用 Azure Key Vault
- 使用 AWS Secrets Manager
- 使用 Kubernetes Secrets
- 使用环境变量注入

## 当前配置优先级
1. User Secrets（最高优先级）
2. appsettings.local.json
3. appsettings.Development.json
4. appsettings.json
