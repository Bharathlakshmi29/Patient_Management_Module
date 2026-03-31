using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Patient_mgt.Infrastructure
{
    public enum QueryIntent
    {
        Guideline,
        PatientData,
        Hybrid
    }

    public class QueryRoutingResult
    {
        public QueryIntent Intent { get; set; }
        public string? PatientName { get; set; }
        public string? Mrn { get; set; }
    }

    public interface IQueryRouter
    {
        QueryRoutingResult Route(string question);
    }

    public class QueryRouter : IQueryRouter
    {
        // -------------------------------
        // Regex patterns
        // -------------------------------
        private static readonly Regex MrnRegex =
            new Regex(@"\b(MRN[-:]?\d+)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Question/filler words to strip before name extraction
        private static readonly HashSet<string> QueryStopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "what","is","the","of","for","a","an","in","on","at","to","by","with",
            "show","get","give","tell","me","us","find","fetch","display","list",
            "latest","recent","last","current","new","old","all","any","this","that",
            "his","her","their","my","your","its","about","from","how","when","where",
            "which","who","does","do","did","has","have","had","was","were","are","been"
        };

        private static readonly string[] PatientKeywords =
        {
            "patient","mrn","id","record","history","report","lab",
            "vitals","prescription","medication","allergy",
            "diagnosis","bp","blood","sugar","hba1c",
            "condition","status","age","dob","medicine","visit",
            "emr","info","detail","summary","analysis","test",
            "result","weight","height","temperature","pulse"
        };

        private static readonly string[] GuidelineKeywords =
        {
            "treatment","management","guideline","dose","dosage",
            "symptoms","causes","therapy","recommend",
            "what should we do","how to treat","plan"
        };

        // -------------------------------
        // PUBLIC ROUTER METHOD
        // -------------------------------
        public QueryRoutingResult Route(string question)
        {
            var result = new QueryRoutingResult();

            bool hasMrn = TryExtractMrn(question, out string? mrn);
            bool hasPatientKeyword = ContainsKeywords(question, PatientKeywords);
            bool hasGuidelineKeyword = ContainsKeywords(question, GuidelineKeywords);
            bool hasName = TryExtractPatientName(question, out string? name);

            // Name is a patient signal when accompanied by a patient keyword or MRN,
            // OR when the entire query is just a name (pure name lookup like "meghna sri"),
            // OR when a name appears alongside guideline keywords (e.g. "meghna sri treatment" = Hybrid)
            bool isNameOnlyQuery = hasName && !string.IsNullOrWhiteSpace(name)
                && question.Trim().Equals(name, StringComparison.OrdinalIgnoreCase);
            bool hasPatientSignal = hasMrn || hasPatientKeyword || isNameOnlyQuery
                || (hasName && (hasMrn || hasPatientKeyword || hasGuidelineKeyword));
            bool hasGuidelineSignal = hasGuidelineKeyword;

            // Intent logic
            if (hasPatientSignal && hasGuidelineSignal)
                result.Intent = QueryIntent.Hybrid;
            else if (hasPatientSignal)
                result.Intent = QueryIntent.PatientData;
            else
                result.Intent = QueryIntent.Guideline;

            result.PatientName = name;
            result.Mrn = mrn;

            return result;
        }

        // -------------------------------
        // MRN Extraction
        // -------------------------------
        private bool TryExtractMrn(string question, out string? mrn)
        {
            var match = MrnRegex.Match(question);
            if (match.Success)
            {
                mrn = match.Groups[1].Value.ToUpper();
                return true;
            }

            mrn = null;
            return false;
        }

        // -------------------------------
        // Patient Name Extraction
        // -------------------------------
        private bool TryExtractPatientName(string question, out string? name)
        {
            var allStopWords = new HashSet<string>(
                PatientKeywords.Concat(GuidelineKeywords).Concat(QueryStopWords),
                StringComparer.OrdinalIgnoreCase);

            // Strip punctuation and split into tokens
            var tokens = Regex.Replace(question, @"[^a-zA-Z\s]", " ")
                              .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Candidate name tokens = tokens that are not stop words and not MRN-like
            var nameTokens = tokens
                .Where(t => !allStopWords.Contains(t) && !Regex.IsMatch(t, @"^MRN", RegexOptions.IgnoreCase))
                .ToList();

            if (nameTokens.Count == 0)
            {
                name = null;
                return false;
            }

            // Take up to 3 consecutive name tokens (first/middle/last name)
            name = string.Join(" ", nameTokens.Take(3));
            return true;
        }

        // -------------------------------
        // Keyword Search Helper
        // -------------------------------
        private bool ContainsKeywords(string text, string[] keywords)
        {
            text = text.ToLower();
            return keywords.Any(k => text.Contains(k));
        }
    }
}
