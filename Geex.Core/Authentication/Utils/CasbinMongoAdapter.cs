﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver;

using NetCasbin.Model;
using NetCasbin.Persist;

using Volo.Abp.Domain.Repositories;

namespace Geex.Shared._ShouldMigrateToLib.Auth
{
    public class CasbinMongoAdapter : IAdapter
    {
        public Func<IRepository<CasbinRule>> RuleCollection { get; }

        public CasbinMongoAdapter(Func<IRepository<CasbinRule>> ruleCollection)
        {
            RuleCollection = ruleCollection;
        }

        public void LoadPolicy(Model model)
        {
            var repository = RuleCollection.Invoke();
            var list = repository.ToList();
            LoadPolicyData(model, Helper.LoadPolicyLine, list);
        }

        public Task LoadPolicyAsync(Model model)
        {
            throw new NotImplementedException();
        }

        public void RemovePolicy(string pType, IList<string> rule)
        {
            RemoveFilteredPolicyAsync(pType, 0, rule.ToArray());
        }

        public async Task RemoveFilteredPolicyAsync(
            string ptype,
            int fieldIndex,
            params string[] fieldValues)
        {
            if (fieldValues == null || !fieldValues.Any())
                return;
            var line = new CasbinRule()
            {
                PType = ptype
            };
            var num = fieldValues.Count();
            if (fieldIndex <= 0 && 0 < fieldIndex + num)
                line.V0 = fieldValues[-fieldIndex];
            if (fieldIndex <= 1 && 1 < fieldIndex + num)
                line.V1 = fieldValues[1 - fieldIndex];
            if (fieldIndex <= 2 && 2 < fieldIndex + num)
                line.V2 = fieldValues[2 - fieldIndex];
            if (fieldIndex <= 3 && 3 < fieldIndex + num)
                line.V3 = fieldValues[3 - fieldIndex];
            if (fieldIndex <= 4 && 4 < fieldIndex + num)
                line.V4 = fieldValues[4 - fieldIndex];
            if (fieldIndex <= 5 && 5 < fieldIndex + num)
                line.V5 = fieldValues[5 - fieldIndex];
            await RuleCollection.Invoke().DeleteAsync(x => (fieldIndex <= 0 && 0 < fieldIndex + num && x.V0 == line.V0)
                                                    && (fieldIndex <= 1 && 1 < fieldIndex + num && x.V1 == line.V1)
                                                    && (fieldIndex <= 2 && 2 < fieldIndex + num && x.V2 == line.V2)
                                                    && (fieldIndex <= 3 && 3 < fieldIndex + num && x.V3 == line.V3)
                                                    && (fieldIndex <= 4 && 4 < fieldIndex + num && x.V4 == line.V4));
        }

        public async Task SavePolicyAsync(Model model)
        {
            var source = new List<CasbinRule>();
            if (model.Model.ContainsKey("p"))
            {
                foreach (var keyValuePair in model.Model["p"])
                {
                    var key = keyValuePair.Key;
                    foreach (var stringList in keyValuePair.Value.Policy)
                    {
                        var casbinRule = savePolicyLine(key, stringList);
                        source.Add(casbinRule);
                    }
                }
            }
            if (model.Model.ContainsKey("g"))
            {
                foreach (var keyValuePair in model.Model["g"])
                {
                    var key = keyValuePair.Key;
                    foreach (var stringList in keyValuePair.Value.Policy)
                    {
                        var casbinRule = savePolicyLine(key, stringList);
                        source.Add(casbinRule);
                    }
                }
            }
            if (!source.Any())
                return;
            foreach (var x in source)
            {
                await RuleCollection.Invoke().InsertAsync(x);
            }
        }

        void IAdapter.AddPolicy(string sec, string ptype, IList<string> rule)
        {
            this.AddPolicyAsync(ptype, rule);
        }

        public Task AddPolicyAsync(string sec, string ptype, IList<string> rule)
        {
            this.AddPolicyAsync(ptype, rule);
            return Task.CompletedTask;
        }

        void IAdapter.RemovePolicy(string sec, string ptype, IList<string> rule)
        {
            this.RemovePolicy(ptype, rule);
        }

        public Task RemovePolicyAsync(string sec, string ptype, IList<string> rule)
        {
            this.RemovePolicy(ptype, rule);
            return Task.CompletedTask;
        }

        void IAdapter.RemoveFilteredPolicy(string sec, string ptype, int fieldIndex, params string[] fieldValues)
        {
            this.RemoveFilteredPolicyAsync(ptype, fieldIndex, fieldValues);
        }

        public Task RemoveFilteredPolicyAsync(string sec, string ptype, int fieldIndex, params string[] fieldValues)
        {
            this.RemoveFilteredPolicyAsync(ptype, fieldIndex, fieldValues);
            return Task.CompletedTask;
        }

        public async Task AddPolicyAsync(string pType, IList<string> rule)
        {
            await RuleCollection.Invoke().InsertAsync(savePolicyLine(pType, rule));
        }

        private void LoadPolicyData(
            Model model,
            Helper.LoadPolicyLineHandler<string, Model> handler,
            IEnumerable<CasbinRule> rules)
        {
            foreach (var rule in rules)
                handler(GetPolicyCotent(rule), model);
        }

        private string GetPolicyCotent(CasbinRule rule)
        {
            var sb = new StringBuilder(rule.PType);
            Append(rule.V0);
            Append(rule.V1);
            Append(rule.V2);
            Append(rule.V3);
            Append(rule.V4);
            Append(rule.V5);
            return sb.ToString();

            void Append(string v)
            {
                if (string.IsNullOrEmpty(v))
                    return;
                sb.Append(", " + v);
            }
        }

        private CasbinRule savePolicyLine(string pType, IList<string> rule)
        {
            var casbinRule = new CasbinRule();
            casbinRule.PType = pType;
            if (rule.Count() > 0)
                casbinRule.V0 = rule[0];
            if (rule.Count() > 1)
                casbinRule.V1 = rule[1];
            if (rule.Count() > 2)
                casbinRule.V2 = rule[2];
            if (rule.Count() > 3)
                casbinRule.V3 = rule[3];
            if (rule.Count() > 4)
                casbinRule.V4 = rule[4];
            if (rule.Count() > 5)
                casbinRule.V5 = rule[5];
            return casbinRule;
        }

        public void SavePolicy(Model model)
        {
            this.SavePolicyAsync(model).Wait();
        }
    }
}