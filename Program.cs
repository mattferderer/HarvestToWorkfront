using System.Text.Json;
using System.Text.RegularExpressions;

namespace HarvestToWorkfront;
class Program
{
    static readonly HttpClient client = new HttpClient();
    static async Task Main(string[] args)
    {
        string startDate = "2024-02-19"; // Set your date range
        string endDate = "2024-02-24";
        // Workfront cookie - needs to be copied/pasted each time
        var cookie = $"quicksilver=true; attask=fa88b039f9c33027b1b4384f611e9883; wf-auth=eyJhbGciOiJSUzI1NiIsImtpZCI6Inp2YThKMG9OWWZIRGJrWHFtRy1WVWsxUkZ4TDg1NUFjYi1CNFRwVV94M0UifQ.eyJleHAiOjE3MDg5OTg4NTgsImlhdCI6MTcwODkxMjQ1OSwiaXNzIjoid29ya2Zyb250Iiwic3ViIjoiOUYyRDVGMjk2NTBBMTg5RjBBNDk1QzRGQDlhYTc1YjU2NjUwOWZjZWQwYTQ5NWVhNCIsImF1ZCI6Indvcmtmcm9udCIsIndvcmtmcm9udCI6eyJlbWFpbCI6Im1mZXJkZXJlckBuZXh1c2lubm92YXRpb25zLmNvbSIsImRpc3BsYXlfbmFtZSI6Ik1hdHQgRmVyZGVyZXIiLCJzdWJkb21haW4iOiJuZXh1cyIsInRlbmFudF9pZCI6ImY5NGEyYmNkZjA2ODQ3ZGRhZGIzYTZjZTY2YmRjNzZiIiwiaW1zX2p3dCI6ImV5SmhiR2NpT2lKU1V6STFOaUlzSW5nMWRTSTZJbWx0YzE5dVlURXRhMlY1TFdGMExURXVZMlZ5SWl3aWEybGtJam9pYVcxelgyNWhNUzFyWlhrdFlYUXRNU0lzSW1sMGRDSTZJbUYwSW4wLmV5SnBaQ0k2SWpFM01EZzVNVEkwTlRnNE5EUmZOVFUxTVRSbFl6QXROelJsTnkwMFlqWmpMVGhrTjJVdE5XSmtOemhrWkRnMk1UazNYM1YzTWlJc0luUjVjR1VpT2lKaFkyTmxjM05mZEc5clpXNGlMQ0pqYkdsbGJuUmZhV1FpT2lKM2IzSnJabkp2Ym5RdGIybGtZeUlzSW5WelpYSmZhV1FpT2lJNVJqSkVOVVl5T1RZMU1FRXhPRGxHTUVFME9UVkRORVpBT1dGaE56VmlOVFkyTlRBNVptTmxaREJoTkRrMVpXRTBJaXdpYzNSaGRHVWlPaUppWVRZNU5UYzJNeTFsWldFMUxUUmpOakl0T1dOaU1TMDFPVEpoT1dVNU9XUTBPV1VpTENKaGN5STZJbWx0Y3kxdVlURWlMQ0poWVY5cFpDSTZJamxHTWtRMVJqSTVOalV3UVRFNE9VWXdRVFE1TlVNMFJrQTVZV0UzTldJMU5qWTFNRGxtWTJWa01HRTBPVFZsWVRRaUxDSmpkSEFpT2pBc0ltWm5Jam9pV1VoTlJFWmFWekpHVUZBMU5FaFZTMGhOVVZaWlNFRkJSMWs5UFQwOVBUMGlMQ0p6YVdRaU9pSXhOekEzTkRrd01EQXhNVGd6WDJOaFpqQTNNakJqTFRGbE5qRXROR0ZoTWkwNFpqRTFMVEZrWkRnd01qQXpPVEJsTVY5MWR6SWlMQ0p5ZEdsa0lqb2lNVGN3T0RreE1qUTFPRGcwTkY5aE5USmhNVEJsWlMwNFlUSmlMVFEwT1dJdFlUZGhNUzB6WlRjeU1ERTVNemxtT1RWZmRYY3lJaXdpYlc5cElqb2lOemhtWVRFM05EVWlMQ0p3WW1FaU9pSk5aV1JUWldOT2IwVldMRXh2ZDFObFl5SXNJbkowWldFaU9pSXhOekV3TVRJeU1EVTRPRFEwSWl3aVpYaHdhWEpsYzE5cGJpSTZJamcyTkRBd01EQXdJaXdpWTNKbFlYUmxaRjloZENJNklqRTNNRGc1TVRJME5UZzRORFFpTENKelkyOXdaU0k2SW05d1pXNXBaQ3h3Y205bWFXeGxMRzltWm14cGJtVmZZV05qWlhOekxHRmlMbTFoYm1GblpTeGhZMk52ZFc1MFgyTnNkWE4wWlhJdWNtVmhaQ3hoWkdScGRHbHZibUZzWDJsdVptOHVjSEp2YW1WamRHVmtVSEp2WkhWamRFTnZiblJsZUhRc1lXUmthWFJwYjI1aGJGOXBibVp2TG5KdmJHVnpMSEpsWVdSZmIzSm5ZVzVwZW1GMGFXOXVjeXhCWkc5aVpVbEVJbjAuZzdJQndJUXl3N3YzLUZfdzlwd0xSZV82Sk1IQXc2Qy1wRUswREdNU29uekp4blRoZVFiVmN4Z2hjcnJ2Y3doSDNfNHVoX2poUG1VeHU3STRLVGh0MTRsdXd0UUdQYk11S2dHZ3pWaFEtQ1JXZ01GM1dJdFJ1RTN1OUlIMXBRUzB3eTgxZXhIMXJTaC1La21QZ1ZEeC02aWFoaUFzN25feW9lUEc3T05JSlotZnM0b1FFandEeHA0cU5iNkR0SjFSZlpvdjdyWFYxTkxKUUtLZWtuOXZvTU9xNUQtYVBLeUxhR0VpQlNablZ0YVdqS2dfdXJ0djIzMVh2LUw1VDZWSk8tdUMzN01mcDdCbFNZQlNMNlFEMEZVUzRCYlE5R1RIdWhZQi1NZ3ZyRUo1S3JPdnJDSnlYX05sTnlwaVpsUTYyTlpCVTZyNzA0Y1VfVXVWczdmNlF3Iiwic2Vzc2lvbl9pZCI6IjE3MDc0OTAwMDExODNfY2FmMDcyMGMtMWU2MS00YWEyLThmMTUtMWRkODAyMDM5MGUxX3V3MjpmOTRhMmJjZGYwNjg0N2RkYWRiM2E2Y2U2NmJkYzc2YiIsInR5cGUiOiJ3ZWIiLCJycl91c2VyX2lkIjoiNWJlMzA0MTUwMGZhZWI3Y2E4OTRkYTM5YmE4MjY1OWYiLCJycl90ZW5hbnRfaWQiOiI5ZmViNDlkZjNmNTgxMTNhZTA0MDAwN2YwMTAwMjQ4MiIsImlzX3JyX2FkbWluIjpmYWxzZSwiYXV0aF9zeXN0ZW0iOiJJTVMiLCJsYW5lIjoicHJvZCJ9fQ.TfiUgrHaCHxUeuJLFa6svKFMI3UhdlfBpkfmXF2rGWX6c6lUzPD9NfmQv7qwYyZY_vniD1Ui1WXwYV1r_FMjl8PXbMKwUGwQaYEE0tT97gFXl7gCDRzK-txkcEOypKGy8geTPi1EEbAHZG6ZwKkLh8z3ifv4uPm2IqBz997zMztxnCX6rmSlyWdRxhuOQCZXycPwMsLNWI-IPy8-f-dg6oEBsFRRnJsLq0Q8jNXfW_nyBCDf0nb6JSkoi_3Eug09pFl8pCYcKgPhhWZxLK7RISB64rhPGfqKjSllhjja2cq2BEYGOpsxt8_rQLBGpEi3sPrHgF-jUgibb7EAIudBLA; XSRF-TOKEN=df22bd976c584d0ea5a9369c7c70065a; webcache=65dbef4b010691a091a857041536965d; wf-node=540f1a8e-7a8f-4b92-9e1f-8fd24cfc390e; kndctr_6AD033CF62197E1C0A495FDD_AdobeOrg_identity=CiY2OTUwNDQxNjU0MzgyMTEwOTE4MzU4NTA4NTIyMDM5ODU0MzcxN1IQCIyyopPcMRgBKgNWQTYwAfAByov7mN4x; kndctr_6AD033CF62197E1C0A495FDD_AdobeOrg_cluster=va6; _dd_s=rum=2&id=b660d6ad-b5b3-4fd8-a348-745a4e3de948&created=1708912461284&expire=1708913361284; sessionExpiration=1709517260840";

        // Harvest API Token
        string harvestToken = "159679.pt.O3tTleFdBc6f9Zou_4K8su3hkuZGnbqHvtoIj6R9zn94tmvW-hsaSzIkBkEYfmu9QxqBQUl4DvAohAlrJFun5w";
        string harvestAccountId = "322473";
        string workfrontUrl = "https://nexus.my.workfront.com/attask/api-internal/HOUR";

        string harvestUrl = $"https://api.harvestapp.com/api/v2/time_entries?client_id=7391033&is_billed=false&from={startDate}&to={endDate}";

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
    public string hourTypeID { get; set; } = "ca57688b54c3a0375c408f3ccf0af28b";
    public string projectID { get; set; } = "639ca41a001202f05ca314217e030f96";
    public string taskID { get; set; }
    public string? opTaskID { get; set; }
    public string roleID { get; set; } = "5be3048e00fb419c81b4bf8372c6f49e";
    public double hours { get; set; }
    public double days { get => hours / 8; }
    public string? status { get; set; }
    public string ownerID { get; set; } = "5be3041500faeb7ca894da39ba82659f";
    public string timesheetID { get; set; }
}

public static class WorkfrontTranslator
{
    public static string Translate(long harvestTaskId, string taskName)
    {
        return (harvestTaskId switch
        {
            // 4DM: Data Migration Execution - Patient',
            (long)20520297 => "https://nexus.my.workfront.com/task/63d15b55002464d3c833e23acdc348b2",

            // BCBS 4DM: Project Planning Meetings',
            (long)19906453 => "https://nexus.my.workfront.com/task/63a5b2320002092c0f21f45d86e6d449",

            // BCBS 4DM Data Migration Technical Design - working on technical documentation related to data migration',
            (long)19923960 => "https://nexus.my.workfront.com/task/63a5b23300020937d7dbd35e4d7099f8",

            // BCBS 4DM Data Migration Design Meetings - planning/discussing elements of Data Migration',
            (long)19923959 => "https://nexus.my.workfront.com/task/63ac9df5003650ec2ac96b5d465f7910",

            // 4DM: Data Migration Execution - Provider'
            (long)20520310 => "https://nexus.my.workfront.com/task/63d15b2e002458c06b9664e671b319c3",

            // 4DM: Data Migration Execution - Inventory'
            (long)20853853 => "https://nexus.my.workfront.com/task/63d15b3d00245cb1274bae0f60e73c61",
            (long)20836094 => "https://nexus.my.workfront.com/task/63d15b3d00245cb1274bae0f60e73c61",

            // Development Environment Challenges'
            (long)20844064 => "https://nexus.my.workfront.com/task/6483374d000c643a2ac3c6712b502cea",

            // Criminal History'
            (long)19906456 => "https://nexus.my.workfront.com/task/6195349c0013f5f72d6340bbec2f3fe7",

            // Internal Meetings
            // (long)20972134 => "https://nexus.my.workfront.com/tshet/648c1104001dce155c82054f4d1d0204",

            _ => TranslateFromTaskName(harvestTaskId, taskName),

        }).Replace("https://nexus.my.workfront.com/task/", "");
    }

    /// <summary>
    /// If TaskName has a id surrounded by parenthesis, return that.
    /// </summary>
    /// <param name="taskName"></param>
    /// <returns></returns>
    private static string TranslateFromTaskName(long harvestTaskId, string taskName)
    {
        var match = Regex.Match(taskName, @"\(([^)]*)\)");
        if (match.Success)
        {
            return "https://nexus.my.workfront.com/task/" + match.Groups[1].Value;
        }
        else
        {
            throw new Exception($"Missing Workfront translation ID for {harvestTaskId}");
        }
    }
}