﻿using Lucene.Net.QueryParsers.Classic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Skybrud.Umbraco.Search.Options.Fields {

    public class FieldList : IEnumerable<Field> {

        private readonly List<Field> _fields;

        #region Properties

        public static FieldList DefaultFields => GetFromStringArray(new[] { "nodeName_lci", "contentTeasertext_lci", "contentBody_lci" });

        /// <summary>
        /// Gets the amount of fields added to to the list.
        /// </summary>
        public int Count => _fields?.Count ?? 0;

        /// <summary>
        /// Returns whether at least one field has a boost value.
        /// </summary>
        public bool HasBoostValues {
            get { return _fields.Any(x => x.Boost != null && x.Boost != 0); }
        }

        /// <summary>
        /// Returns whether at least one field has a fuzzy value.
        /// </summary>
        public bool HasFuzzyValues {
            get { return _fields.Any(x => x.Fuzz != null && x.Fuzz > 0 && x.Fuzz < 1); }
        }

        /// <summary>
        /// Returns whether the list is valid, which is when the list contain more or more files.
        /// </summary>
        public bool IsValid => _fields != null && _fields.Any();

        /// <summary>
        /// Gets whether the list is empty.
        /// </summary>
        public bool IsEmpty => Count == 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new, empty list.
        /// </summary>
        public FieldList() {
            _fields = new List<Field>();
        }

        /// <summary>
        /// Initializes a new list based on the specified <paramref name="fieldNames"/>.
        /// </summary>
        /// <param name="fieldNames">An array with the names of each field to be added.</param>
        public FieldList(params string[] fieldNames) {
            _fields = new List<Field>();
            foreach (var fieldName in fieldNames) {
                _fields.Add(new Field(fieldName));
            }
        }

        /// <summary>
        /// Initializes a new list based on the specified <paramref name="fieldNames"/>.
        /// </summary>
        /// <param name="fieldNames">A collection with the names of each field to be added.</param>
        public FieldList(IEnumerable<string> fieldNames) {
            _fields = new List<Field>();
            foreach (var fieldName in fieldNames) {
                _fields.Add(new Field(fieldName));
            }
        }

        /// <summary>
        /// Initializes a new list based on the specified <paramref name="fields"/>.
        /// </summary>
        /// <param name="fields">An array with fields to be added.</param>
        public FieldList(params Field[] fields) {
            _fields = new List<Field>(fields);
        }

        /// <summary>
        /// Initializes a new list based on the specified <paramref name="fields"/>.
        /// </summary>
        /// <param name="fields">A collection with fields to be added.</param>
        public FieldList(IEnumerable<Field> fields) {
            _fields = new List<Field>(fields);
        }

        #endregion

        #region Member methods

        /// <summary>
        /// Adds the specified <paramref name="field"/>.
        /// </summary>
        /// <param name="field">The field to be added to the list.</param>
        public void Add(Field field) {
            _fields.Add(field);
        }

        /// <summary>
        /// Adds a new field with the specified <paramref name="fieldName"/>, <paramref name="boost"/> and <paramref name="fuzz"/> values.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="boost">The boost value of the field.</param>
        /// <param name="fuzz">The fuzzy value of the field.</param>
        public void Add(string fieldName, int? boost = null, float? fuzz = null) {
            _fields.Add(new Field(fieldName, boost, fuzz));
        }

        /// <summary>
        /// Adds the specified array of <paramref name="fields"/>.
        /// </summary>
        /// <param name="fields">The fields to be added to the list.</param>
        public void AddRange(params Field[] fields) {
            _fields.AddRange(fields);
        }

        /// <summary>
        /// Adds the specified collections of <paramref name="fields"/>.
        /// </summary>
        /// <param name="fields">The fields to be added to the list.</param>
        public void AddRange(IEnumerable<Field> fields) {
            _fields.AddRange(fields);
        }

        /// <summary>
        /// Returns the raw Examine query for the fields in this list.
        /// </summary>
        /// <param name="terms">The search terms to search for.</param>
        /// <returns>The raw Examine query.</returns>
        public virtual string GetQuery(string[] terms) {

            List<string> searchTerms = new List<string>();

            foreach (string term in terms) {

                string escapedTerm = QueryParserBase.Escape(term);
                string t = "(";

                if (IsValid) {

                    // Boost
                    if (HasBoostValues) {
                        t += string.Join(" OR ", _fields.Where(x => x.Boost != null).Select(fieldOption => string.Format("{0}:({1} {1}*)^{2}", fieldOption.FieldName, escapedTerm, fieldOption.Boost.ToString())).ToArray());
                        t += " OR ";
                    }

                    // Fuzzy
                    if (HasFuzzyValues) {
                        t += string.Join(" OR ", _fields.Where(x => x.Fuzz != null && x.Fuzz > 0 && x.Fuzz < 1).Select(fieldOption => string.Format("{0}:{1}~{2}", fieldOption.FieldName, escapedTerm, fieldOption.Fuzz.ToString())).ToArray());
                        t += " OR ";
                    }

                    // Add regular search
                    t += string.Join(" OR ", _fields.Select(fieldOption => string.Format("{1}:({0} {0}*)", escapedTerm, fieldOption.FieldName)).ToArray());
                }

                t += ")";
                searchTerms.Add(t);

            }

            return string.Join(" AND ", searchTerms.ToArray());

        }

        public IEnumerator<Field> GetEnumerator() {
            return _fields.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Use this for creating a new instance of the fieldoptions class where only the fieldNames are set
        /// </summary>
        /// <param name="fieldNames"></param>
        /// <returns>The created <see cref="FieldList"/> instance.</returns>
        public static FieldList GetFromStringArray(string[] fieldNames) {
            return new FieldList(fieldNames);
        }

        #endregion

    }

}