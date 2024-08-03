using System.Text.Json;
using System.Text.RegularExpressions;

namespace HarvestToWorkfront;
class Program
{
    static readonly HttpClient client = new HttpClient();
    static async Task Main(string[] args)
    {
        // Workfront cookie - needs to be copied/pasted each time
        var cookie = $"";
        string harvestClientId = "1234567";
        string startDate = "2024-07-28"; // Set your date range
        string endDate = "2024-07-31";
        string harvestToken = "159679.pt....";
        string harvestAccountId = "123456";
        string workfrontUrl = "https://abccompany.my.workfront.com/attask/api-internal/HOUR";
        string harvestUrl = $"https://api.harvestapp.com/api/v2/time_entries?client_id={harvestClientId}&is_billed=false&from={startDate}&to={endDate}";

        var harvestRequest = new HttpRequestMessage(HttpMethod.Get, harvestUrl);
        harvestRequest.Headers.Add("Authorization", "Bearer " + harvestToken);
        harvestRequest.Headers.Add("Harvest-Account-ID", harvestAccountId);
        harvestRequest.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36");

        var harvestResponse = await client.SendAsync(harvestRequest);

        var harvestData = await harvestResponse.Content.ReadAsStringAsync();
        var harvestJson = JsonSerializer.Deserialize<HarvestResponse>(harvestData);

        var groupedTimeEntries = harvestJson.time_entries
            .GroupBy(e => new { e.spent_date, e.task.id });

        foreach (var group in groupedTimeEntries)
        {
            var workfrontData = new WorkfrontTimeEntry
            {
                entryDate = group.Key.spent_date,
                //TODO USE ROUNDED HOURS
                hours = group.Sum(e => e.rounded_hours),
                description = string.Join(" ", group.Select(e => e.notes)),
                projectID = group.First().project.id.ToString(),
            };
            try
            {
                workfrontData.taskID = WorkfrontTranslator.Translate(group.Key.id, group.First().task.name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " ~ " + $"Date: {group.Key.spent_date} | TaskId: {group.Key.id} | Description: {string.Join(" ", group.Select(e => e.notes))}");
                continue;
            }

            var workfrontUpdates = new { createHours = new[] { workfrontData }, updateHours = new List<string>(), deleteHours = new List<string>() };
            var workfrontPayload =
                new Dictionary<string, string>
            {
                { "updates", JsonSerializer.Serialize(workfrontUpdates) },
                { "method", "PUT" },
                { "action", "batchSave" }
            };

            //new { updates = JsonSerializer.Serialize(workfrontUpdates), method = "PUT", action = "batchSave" };

            var workfrontRequest = new HttpRequestMessage(HttpMethod.Post, workfrontUrl);
            workfrontRequest.Headers.Add("Cookie", cookie);
            workfrontRequest.Headers.Add("x-xsrf-token", GetValuesFromCookie(cookie)["XSRF-TOKEN"]);

            workfrontRequest.Content = new FormUrlEncodedContent(workfrontPayload);

            var workfrontResponse = await client.SendAsync(workfrontRequest);
            var workfrontResponseData = await workfrontResponse.Content.ReadAsStringAsync();

            Console.WriteLine(workfrontResponseData);
        }
    }

    private static Dictionary<string, string> GetValuesFromCookie(string cookie)
    {
        var dictionary = new Dictionary<string, string>();

        foreach (var item in cookie.Split(';'))
        {
            var parts = item.Trim().Split(new[] { '=' }, 2); // Splitting only at the first '='
            if (parts.Length == 2)
            {
                dictionary[parts[0].Trim()] = parts[1].Trim();
            }
        }
        return dictionary;
    }
}

public class HarvestTimeEntry
{
    public long id { get; set; }
    public string spent_date { get; set; }
    public double hours { get; set; }
    public double rounded_hours { get; set; }
    public string notes { get; set; }
    public User user { get; set; }
    public Client client { get; set; }
    public Project project { get; set; }
    public Task task { get; set; }

    public class User
    {
        public long id { get; set; }
        public string name { get; set; }
    }

    public class Client
    {
        public long id { get; set; }
        public string name { get; set; }
        public string currency { get; set; }
    }

    public class Project
    {
        public long id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
    }

    public class Task
    {
        public long id { get; set; }
        public string name { get; set; }
    }
}
public class HarvestResponse
{
    public HarvestTimeEntry[] time_entries { get; set; }
}
public class WorkfrontTimeEntry
{
    public string referenceObjCode { get; set; } = "TASK";
    //public string referenceObjID { get; set; }
    //public string timesheetHourIdentifier { get; set; }
    public string entryDate { get; set; }
    public string? ID { get; set; }
    public string description { get; set; }
    public string hourTypeID { get; set; } = "";
    public string projectID { get; set; } = "";
    public string taskID { get; set; }
    public string? opTaskID { get; set; }
    public string roleID { get; set; } = "";
    public double hours { get; set; }
    public double days { get => hours / 8; }
    public string? status { get; set; }
    public string ownerID { get; set; } = "";
    public string timesheetID { get; set; }
}

public static class WorkfrontTranslator
{
    const string url = "https://abccompany.my.workfront.com/task/";
    public static string Translate(long harvestTaskId, string taskName)
    {
        return TranslateFromTaskName(harvestTaskId, taskName)
            .Replace(url, "");
    }

    /// <summary>
    /// If TaskName has a id surrounded by parenthesis, return that. Example:   Database Migration (023489023423809)
    /// </summary>
    /// <param name="taskName"></param>
    /// <returns></returns>
    private static string TranslateFromTaskName(long harvestTaskId, string taskName)
    {
        var match = Regex.Match(taskName, @"\(([^)]*)\)");
        if (match.Success)
        {
            return url + match.Groups[1].Value;
        }
        else
        {
            throw new Exception($"Missing Workfront translation ID for {harvestTaskId}");
        }
    }
}