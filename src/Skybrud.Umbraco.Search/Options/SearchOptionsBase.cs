﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Skybrud.Umbraco.Search.Models.Options;
using Umbraco.Core;

namespace Skybrud.Umbraco.Search.Options {

    public class SearchOptionsBase : IPaginatedSearchOptions {

        #region Properties

        public string ExamineSearcher { get; set; }
        
        /// <summary>
        /// Gets or sets the text to search for.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a list of fields that should be used when searching for <see cref="Fields"/>.
        /// </summary>
        public FieldList TextFields { get; set; }

        /// <summary>
        /// Gets or sets an array of IDs the returned results should be a descendant of. At least one of the IDs should
        /// be in the path of the result to be a match.
        /// </summary>
        public int[] RootIds { get; set; }

        /// <summary>
        /// Gets or sets the content types the returned results should match.
        /// </summary>
        public ContentTypeList ContentTypes { get; set; }

        public int Offset { get; set; }

        public int Limit { get; set; }

        /// <summary>
        /// Gets or sets whether the check on the <c>hideFromSearch</c> field should be disabled.
        /// </summary>
        public bool DisableHideFromSearch { get; set; }

        #endregion

        #region Constructors

        public SearchOptionsBase() {
            ExamineSearcher = Constants.Examine.ExternalSearcher;
            Text = string.Empty;
            RootIds = null;
            ContentTypes = new ContentTypeList();
            TextFields = new FieldList();
        }

        #endregion

        #region Member methods

        public string GetRawQuery() {
            return string.Join(" AND ", GetQueryList());
        }

        protected virtual List<string> GetQueryList() {

            List<string> query = new List<string>();

            SearchType(query);
            SearchText(query);
            SearchRootIds(query);
            SearchHideFromSearch(query);

            return query;

        }

        protected virtual void SearchType(List<string> query) {
            if (ContentTypes == null || ContentTypes.Count == 0) return;
            query.Add($"nodeTypeAlias:({string.Join(" ", ContentTypes.Select(a => $"\"{QueryParser.Escape(a)}\"").ToArray())})");
        }

        protected virtual void SearchText(List<string> query) {

            if (string.IsNullOrWhiteSpace(Text)) return;

            Text = Regex.Replace(Text, @"[^\wæøåÆØÅ\- ]", string.Empty).ToLowerInvariant().Trim();

            string[] terms = Text.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            // fjerner stop-ord fra søgetermen
            terms = terms.Where(x => !StopAnalyzer.ENGLISH_STOP_WORDS_SET.Contains(x.ToLower())).ToArray();

            // fallback if no fields are added
            TextFields = TextFields ?? FieldList.GetFromStringArray(new[] { "nodeName_lci", "contentTeasertext_lci", "contentBody_lci" });

            query.Add(TextFields.GetQuery(terms));

        }

        protected virtual void SearchRootIds(List<string> query) {
            if (RootIds == null || RootIds.Length == 0) return;
            query.Add("(" + string.Join(" OR ", from id in RootIds select "path_search:" + id) + ")");
        }

        protected virtual void SearchHideFromSearch(List<string> query) {
            if (DisableHideFromSearch) return;
            query.Add("hideFromSearch:(0)");
        }

        #endregion

    }

}