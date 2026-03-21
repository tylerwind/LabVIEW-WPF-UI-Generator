# Antigravity Superpowers

这个目录包含了从 [Superpowers](https://github.com/obra/superpowers) 移植到 **Antigravity** 环境的核心能力集。

## 什么是 Superpowers？
Superpowers 是一套结构化的软件开发工作流，通过一系列互相关联的 **Skills (技能)** 和 **Workflows (工作流)**，强制 Agent 遵循最佳工程实践（如 TDD、系统化调试和结构化设计）。

## 如何在 Antigravity 中使用？

### 1. 安装与设置
Antigravity 依赖项目根目录下的 `.agent` 目录来识别技能和工作流。
- **目录要求**：请将本仓库中的 `skills` 、 `workflows`和 `rules` 文件夹移动到项目根目录的 `.agent/` 目录下（如果该目录不存在，请手动创建）。
- **刷新配置**：在移动或更新 Workflows 后，您需要前往 Antigravity 的 **Customizations** 页面点击 **Refresh** 按钮，以确保斜杠命令生效。

### 2. 自动技能加载 (Skills)
Antigravity 会自动检索 `.agent/skills/` 目录。在处理任务时，Agent 会根据需要自动加载并遵循相关技能指令。
- **强制规则**：当你开始开发或调试时，Agent 会根据上下文自动读取并执行对应的 `SKILL.md`。

### 3. 斜杠命令 (Workflows)
你可以直接在聊天框输入以下命令启动标准流程：
- `/brainstorm`：启动需求头脑风暴，产出 Specs。
- `/write-plan`：基于 Specs 编写详细的原子任务清单。
- `/execute-plan`：批量执行任务计划并进行阶段性审查。
- `/git-commit`：按照规范生成中文提交信息。

---
*注：本项目中的能力集基于 Superpowers v4.x 移植。*
