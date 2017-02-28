﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Templates.Core.Injection.References
{
    public class ReferencesInjector : ContentInjector<ReferencesInjectorConfig>
    {
        public ReferencesInjector(string filePath) : base(filePath)
        {
        }

        public ReferencesInjector(ReferencesInjectorConfig config) : base(config)
        {
        }

        public override string Inject(string sourceContent)
        {
            //TODO: VERIFY SAME FORMAT
            var projectJson = JsonConvert.DeserializeObject<ProjectJson>(sourceContent);

            if (projectJson.dependencies == null)
            {
                projectJson.dependencies = new Dictionary<string, string>();
            }

            foreach (var dependency in Config.dependencies)
            {
                projectJson.dependencies.Add(dependency.Key, dependency.Value);
            }

            return JsonConvert.SerializeObject(projectJson, Formatting.Indented);
        }
    }

    public class ProjectJson
    {
        public Dictionary<string, string> dependencies { get; set; }
        public Dictionary<string, object> frameworks { get; set; }
        public Dictionary<string, object> runtimes { get; set; }
    }
}