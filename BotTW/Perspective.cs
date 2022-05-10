using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BotTW
{


    class Perspective
    {
        Message msg;

        HttpClient httpClient;
        HttpRequestMessage request;
        public Perspective(Message _msg)
        {
            msg = _msg;
            httpClient = new HttpClient();
            
        }


        public async Task<float> GetToxicityScore(string text)
        {
            float toxicityScore = 0;
            try
            {
                request = new HttpRequestMessage(new HttpMethod("POST"),
                "https://commentanalyzer.googleapis.com/v1alpha1/comments:analyze?key=TOKEN");
                request.Content = new StringContent("{comment: {text: \"" + text.Replace(@"""", "'").Replace(@"\", "/") + "\"},\n" +
                                                    "languages: [\"ru\"],\n" +
                                                    "requestedAttributes: {TOXICITY:{}} }");

                var response = await httpClient.SendAsync(request);
                Response myDeserializedClass = JsonConvert.DeserializeObject<Response>(await response.Content.ReadAsStringAsync());

                toxicityScore = myDeserializedClass.attributeScores.TOXICITY.summaryScore.value;
            }
            catch
            {
                //msg.AddMessage("I_am_D0BR0 help me, daddy AYAYA ");
            }
            return toxicityScore;

        }
    }
}

public class Score
{
    public float value { get; set; }
    public string type { get; set; }
}

public class SpanScore
{
    public int begin { get; set; }
    public int end { get; set; }
    public Score score { get; set; }
}

public class SummaryScore
{
    public float value { get; set; }
    public string type { get; set; }
}

public class TOXICITY
{
    public List<SpanScore> spanScores { get; set; }
    public SummaryScore summaryScore { get; set; }
}

public class AttributeScores
{
    public TOXICITY TOXICITY { get; set; }
}

public class Response
{
    public AttributeScores attributeScores { get; set; }
    public List<string> languages { get; set; }
    public List<string> detectedLanguages { get; set; }
}