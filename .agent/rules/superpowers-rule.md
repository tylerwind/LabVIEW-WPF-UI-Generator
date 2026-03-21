---
trigger: always_on
---

# Superpowers Development Rules

## 核心准则 (Prime Directives)
- **技能优先**：在处理任何任务前，优先加载并阅读 `.agent/skills/using-superpowers/SKILL.md`。
- **强制 TDD**：所有功能开发必须遵循 `.agent/skills/test-driven-development/SKILL.md` 的红-绿-重构循环。
- **结构化设计**：在编写代码前，必须通过 `/brainstorm` 明确设计规范。
- **小步快跑**：所有实施计划必须通过 `/write-plan` 拆分为极小的原子任务。

## 常用命令 (Workflows)
- `/brainstorm`: 启动需求头脑风暴。
- `/write-plan`: 创建实施计划。
- `/execute-plan`: 执行并审查计划。
- `/git-commit`: 规范化提交更改。

## 技术规范
- 语言：Typescript / Node.js
- 提交：Conventional Commits (中文)