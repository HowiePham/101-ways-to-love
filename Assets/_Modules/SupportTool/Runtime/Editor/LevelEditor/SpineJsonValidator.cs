using System.IO;
using UnityEditor;
using UnityEngine;

public class SpineJsonValidator : EditorWindow
{
    private string jsonFilePath = "";
    private TextAsset jsonFile;

    [MenuItem("Tools/Spine JSON Validator")]
    public static void ShowWindow()
    {
        GetWindow<SpineJsonValidator>("Spine JSON Validator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Spine JSON Validator & Fixer", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Select JSON File:");
        jsonFile = (TextAsset)EditorGUILayout.ObjectField(jsonFile, typeof(TextAsset), false);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Validate JSON", GUILayout.Height(30)))
        {
            ValidateJson();
        }

        if (GUILayout.Button("Fix Common Issues", GUILayout.Height(30)))
        {
            FixCommonIssues();
        }

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Open JSON in System Editor", GUILayout.Height(30)))
        {
            if (jsonFile != null)
            {
                string path = AssetDatabase.GetAssetPath(jsonFile);
                System.Diagnostics.Process.Start(path);
            }
        }
    }

    private void ValidateJson()
    {
        if (jsonFile == null)
        {
            Debug.LogError("Please select a JSON file first!");
            return;
        }

        string path = AssetDatabase.GetAssetPath(jsonFile);
        string content = File.ReadAllText(path);

        Debug.Log($"Validating: {path}");
        Debug.Log($"File size: {content.Length} characters");

        // Check for common issues
        int issueCount = 0;

        // 1. Check for duplicate commas
        if (content.Contains(",,"))
        {
            Debug.LogError("Found duplicate commas (,,)");
            issueCount++;
        }

        // 2. Check for missing commas before "events"
        if (content.Contains("]\n    \"events\"") || content.Contains("]\r\n    \"events\""))
        {
            Debug.LogError("Missing comma before 'events' array");
            issueCount++;
        }

        // 3. Check for trailing commas before closing braces
        if (content.Contains(",\n}") || content.Contains(",\r\n}") || content.Contains(",\n  }") || content.Contains(",\r\n  }"))
        {
            Debug.LogWarning("Found trailing commas before closing braces (this may or may not be an issue)");
        }

        // 4. Check events section format
        int eventsIndex = content.IndexOf("\"events\":");
        if (eventsIndex > 0)
        {
            // Get 100 chars after "events":
            string eventsPreview = content.Substring(eventsIndex, Mathf.Min(200, content.Length - eventsIndex));
            Debug.Log($"Events section preview:\n{eventsPreview}");

            // Check if it's all on one line (bad)
            if (eventsPreview.Contains("{") && eventsPreview.IndexOf('\n') > 100)
            {
                Debug.LogError("Events section appears to be on a single line (should have line breaks)");
                issueCount++;
            }
        }

        // 5. Try to parse with Unity's JsonUtility (limited but catches basic errors)
        try
        {
            // Just check if it starts and ends properly
            if (!content.TrimStart().StartsWith("{"))
            {
                Debug.LogError("JSON doesn't start with '{'");
                issueCount++;
            }
            if (!content.TrimEnd().EndsWith("}"))
            {
                Debug.LogError("JSON doesn't end with '}'");
                issueCount++;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON parsing error: {e.Message}");
            issueCount++;
        }

        // 6. Check bracket balance
        int openBraces = 0;
        int closeBraces = 0;
        int openBrackets = 0;
        int closeBrackets = 0;
        bool inString = false;

        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];
            
            if (c == '"' && (i == 0 || content[i - 1] != '\\'))
            {
                inString = !inString;
            }
            
            if (!inString)
            {
                if (c == '{') openBraces++;
                else if (c == '}') closeBraces++;
                else if (c == '[') openBrackets++;
                else if (c == ']') closeBrackets++;
            }
        }

        Debug.Log($"Bracket balance: {{ {openBraces} : {closeBraces} }} [ {openBrackets} : {closeBrackets} ]");

        if (openBraces != closeBraces)
        {
            Debug.LogError($"Unbalanced braces! Open: {openBraces}, Close: {closeBraces}");
            issueCount++;
        }

        if (openBrackets != closeBrackets)
        {
            Debug.LogError($"Unbalanced brackets! Open: {openBrackets}, Close: {closeBrackets}");
            issueCount++;
        }

        // Summary
        EditorGUILayout.Space(10);
        if (issueCount == 0)
        {
            Debug.Log("✓ No obvious issues found! But Spine may still have specific requirements.");
        }
        else
        {
            Debug.LogError($"✗ Found {issueCount} potential issues. Use 'Fix Common Issues' button to attempt auto-fix.");
        }
    }

    private void FixCommonIssues()
    {
        if (jsonFile == null)
        {
            Debug.LogError("Please select a JSON file first!");
            return;
        }

        string path = AssetDatabase.GetAssetPath(jsonFile);
        string content = File.ReadAllText(path);
        string originalContent = content;

        Debug.Log($"Attempting to fix: {path}");

        // Fix 1: Remove duplicate commas
        content = content.Replace(",,", ",");

        // Fix 2: Add comma before "events" if missing
        content = content.Replace("]\n    \"events\":", "],\n    \"events\":");
        content = content.Replace("]\r\n    \"events\":", "],\r\n    \"events\":");
        content = content.Replace("]\n\t\"events\":", "],\n\t\"events\":");
        content = content.Replace("]\r\n\t\"events\":", "],\r\n\t\"events\":");

        // Fix 3: Remove trailing commas before closing braces
        content = System.Text.RegularExpressions.Regex.Replace(content, ",\\s*}", "}");
        content = System.Text.RegularExpressions.Regex.Replace(content, ",\\s*]", "]");

        // Fix 4: Check if events section is malformed (all on one line)
        int eventsStart = content.IndexOf("\"events\":");
        if (eventsStart > 0)
        {
            int eventsOpenBrace = content.IndexOf("{", eventsStart);
            int eventsCloseBrace = -1;
            
            if (eventsOpenBrace > 0)
            {
                int depth = 1;
                bool inString = false;
                
                for (int i = eventsOpenBrace + 1; i < content.Length && depth > 0; i++)
                {
                    char c = content[i];
                    
                    if (c == '"' && (i == 0 || content[i - 1] != '\\'))
                    {
                        inString = !inString;
                    }
                    
                    if (!inString)
                    {
                        if (c == '{') depth++;
                        else if (c == '}')
                        {
                            depth--;
                            if (depth == 0)
                            {
                                eventsCloseBrace = i;
                                break;
                            }
                        }
                    }
                }

                if (eventsCloseBrace > eventsOpenBrace)
                {
                    string eventsSection = content.Substring(eventsOpenBrace, eventsCloseBrace - eventsOpenBrace + 1);
                    
                    // Check if it's all on one line
                    if (!eventsSection.Contains("\n") && eventsSection.Length > 50)
                    {
                        Debug.Log("Reformatting events section...");
                        
                        // Reformat with line breaks
                        string reformatted = "{\n";
                        string[] parts = eventsSection.Trim('{', '}').Split(new[] { "},\"" }, System.StringSplitOptions.None);
                        
                        for (int i = 0; i < parts.Length; i++)
                        {
                            string part = parts[i];
                            if (i > 0) part = "\"" + part;
                            if (i < parts.Length - 1) part = part + "}";
                            
                            reformatted += "\t" + part;
                            if (i < parts.Length - 1) reformatted += ",";
                            reformatted += "\n";
                        }
                        reformatted += "}";
                        
                        content = content.Substring(0, eventsOpenBrace) + reformatted + content.Substring(eventsCloseBrace + 1);
                    }
                }
            }
        }

        // Save if changes were made
        if (content != originalContent)
        {
            string backupPath = path.Replace(".json", "_backup.json");
            File.WriteAllText(backupPath, originalContent);
            Debug.Log($"Created backup: {backupPath}");

            File.WriteAllText(path, content);
            AssetDatabase.Refresh();

            Debug.Log($"✓ Fixed and saved to: {path}");
            Debug.Log("Please validate again to check if all issues are resolved.");
        }
        else
        {
            Debug.Log("No changes needed or no automatic fixes available.");
        }
    }
}