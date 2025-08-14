using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UiPath.CodedWorkflows;

namespace GetIdeaDetails
{
    public class Flatten : CodedWorkflowBase
    {
        [Workflow]
        public JObject Execute(JObject responseObject)
        {
            var result = new JObject();

            try
            {
                if (responseObject == null)
                    return new JObject { ["error"] = "Response JObject is null" };

                var fields = responseObject["fields"] as JObject;
                if (fields == null)
                    return new JObject { ["error"] = "No fields object found" };

                string SafeGet(JToken parent, string property) =>
                    parent is JObject obj && obj[property] != null && obj[property].Type != JTokenType.Null
                        ? obj[property].ToString()
                        : string.Empty;

                // Base properties
                result["key"] = SafeGet(responseObject, "key");
                result["summary"] = SafeGet(fields, "summary");

                // Handle description (ADF JSON or plain text)
                var descriptionToken = fields["description"];
                if (descriptionToken is JObject adf && adf["content"] is JArray contentArray)
                {
                    var sb = new StringBuilder();
                    ParseContent(contentArray, sb, 0);
                    result["description"] = sb.ToString().Trim();
                }
                else if (descriptionToken != null && descriptionToken.Type == JTokenType.String)
                {
                    result["description"] = descriptionToken.ToString();
                }
                else
                {
                    result["description"] = string.Empty;
                }

                // Status & custom fields
                result["status"] = SafeGet(fields["statusCategory"], "name");
                result["value"] = SafeGet(fields, "customfield_17760");
                result["impact"] = SafeGet(fields, "customfield_17741");
                result["demand"] = SafeGet(fields, "customfield_18458");
                result["effort"] = SafeGet(fields, "customfield_17761");
                result["customerPriority"] = SafeGet(fields["customfield_18218"], "value");

                // Categorized linked issues
                var insights = new List<JObject>();
                var delivery = new List<JObject>();
                var connections = new List<JObject>();

                var issueLinks = fields["issuelinks"] as JArray;
                if (issueLinks != null)
                {
                    foreach (var link in issueLinks)
                    {
                        string linkTypeName = link["type"]?["name"]?.ToString()?.ToLower() ?? "";
                        string inwardDesc = link["type"]?["inward"]?.ToString()?.ToLower() ?? "";
                        string outwardDesc = link["type"]?["outward"]?.ToString()?.ToLower() ?? "";

                        JObject linkedIssue = null;
                        string direction = "";
                        if (link["inwardIssue"] != null)
                        {
                            linkedIssue = link["inwardIssue"] as JObject;
                            direction = "inward";
                        }
                        else if (link["outwardIssue"] != null)
                        {
                            linkedIssue = link["outwardIssue"] as JObject;
                            direction = "outward";
                        }

                        if (linkedIssue == null) continue;

                        var issueObj = new JObject
                        {
                            ["key"] = linkedIssue["key"]?.ToString() ?? "",
                            ["issuetype-name"] = linkedIssue["fields"]?["issuetype"]?["name"]?.ToString() ?? "",
                            ["status-name"] = linkedIssue["fields"]?["status"]?["name"]?.ToString() ?? ""
                        };

                        // Categorization rules
                        if (linkTypeName.Contains("discovery - connected") ||
                            linkTypeName.Contains("treatment") ||
                            inwardDesc.Contains("is connected to") ||
                            outwardDesc.Contains("connects to"))
                        {
                            connections.Add(issueObj);
                        }
                        else if (direction == "outward" && issueObj["issuetype-name"]?.ToString().Equals("Insight", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            insights.Add(issueObj);
                        }
                        else if (direction == "inward")
                        {
                            delivery.Add(issueObj);
                        }
                    }
                }

                // Deduplicate by key
                result["Insights"] = new JArray(insights.GroupBy(i => i["key"]?.ToString()).Select(g => g.First()));
                result["Delivery"] = new JArray(delivery.GroupBy(i => i["key"]?.ToString()).Select(g => g.First()));
                result["Connections"] = new JArray(connections.GroupBy(i => i["key"]?.ToString()).Select(g => g.First()));
            }
            catch (Exception ex)
            {
                result = new JObject { ["error"] = $"[ERROR parsing response] {ex.Message}" };
            }

            return result;
        }

        private void ParseContent(JArray contentArray, StringBuilder sb, int indent)
        {
            if (contentArray == null) return;

            foreach (var item in contentArray)
            {
                var type = item?["type"]?.ToString();
                switch (type)
                {
                    case "heading":
                        var level = item["attrs"]?["level"]?.ToObject<int>() ?? 1;
                        sb.AppendLine(new string('#', level) + " " + ExtractText(item["content"] as JArray));
                        sb.AppendLine();
                        break;
                    case "paragraph":
                        sb.AppendLine(ExtractText(item["content"] as JArray));
                        sb.AppendLine();
                        break;
                    case "bulletList":
                        ParseBulletList(item["content"] as JArray, sb, indent);
                        break;
                    case "orderedList":
                        ParseOrderedList(item["content"] as JArray, sb, indent);
                        break;
                    case "code":
                        sb.AppendLine("```" + ExtractText(item["content"] as JArray) + "```");
                        sb.AppendLine();
                        break;
                }
            }
        }

        private void ParseBulletList(JArray listItems, StringBuilder sb, int indent)
        {
            if (listItems == null) return;
            foreach (var li in listItems)
            {
                var text = ExtractText(li["content"]?[0]?["content"] as JArray);
                sb.AppendLine($"{new string(' ', indent * 2)}• {text}");
            }
            sb.AppendLine();
        }

        private void ParseOrderedList(JArray listItems, StringBuilder sb, int indent)
        {
            if (listItems == null) return;
            int i = 1;
            foreach (var li in listItems)
            {
                var text = ExtractText(li["content"]?[0]?["content"] as JArray);
                sb.AppendLine($"{new string(' ', indent * 2)}{i}. {text}");
                i++;
            }
            sb.AppendLine();
        }

        private string ExtractText(JArray content)
        {
            if (content == null) return string.Empty;
            var sb = new StringBuilder();
            foreach (var node in content)
            {
                var text = node["text"]?.ToString();
                if (!string.IsNullOrEmpty(text))
                {
                    sb.Append(text);
                }
                else if (node["type"]?.ToString() == "emoji")
                {
                    sb.Append(node["attrs"]?["text"]?.ToString());
                }
            }
            return sb.ToString();
        }
    }
}
