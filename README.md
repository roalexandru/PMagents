# PMagents

A curated collection of **UiPath agents for Product Managers**.  
This is **not** an official UiPath repository — for sample and demonstration purposes only.

## Implemented Agents

- 🔍 **[Jira Product Discovery – Insights Processor](insights_processing/README.md)** — processes discovery items and surfaces insights.
- 📧 **Generate Emails from User Voice Feedback** — drafts outreach emails based on user feedback.
- 🏢 **[Monitor Selected Competitors and Share the Gist](competitor_analyst/README.md)** — tracks competitor activity and summarizes updates.
- 📋 **TIF Analyzer** — reviews TIFs to identify patterns and recommend actions.

## In-Progress / Ideas

- Classify and respond to user voice tickets  
- Review TIFs and generate an action plan  
- Draft PRDs  
- Summarize scientific articles with a focus on automation and agentic workflows  
- Monitor selected competitors and provide timely updates  
- Track LinkedIn communities for emerging trends in automation  
- Draft meeting notes and action items  
- Sprint retrospective synthesizer and ticket creation  
- Agent that analyzes product telemetry or dashboards and highlights new behaviors or shifts in usage  
- Customer pain pattern mining across support tickets, NPS comments, Clari calls, forum, analyst reviews  
- "What's been tried?" knowledge retriever that searches Jira, Confluence, Slack, Glean, emails  
- Sales enablement content generator  
- Personalized daily focus brief summarizing meetings, deadlines, pending reviews, and decisions needed  
- Backlog grooming assistant

## Getting Started

1. Clone the repo:
   ```bash
   git clone https://github.com/roalexandru/PMagents.git
   cd PMagents
   ```
2. Open the chosen agent folder in UiPath Studio.  
3. Install required packages (listed in each agent folder).  
4. Run the workflow.

## Contributing

- Add working agents under **Implemented Agents** with a short README and sample inputs/outputs.  
- Add partial builds under **In-Progress / Ideas** with what's missing.  
- Use Issues/PRs for new ideas or improvements.
