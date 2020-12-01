using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibGit2Sharp.Handlers;

namespace deploy
{
    public class BranchesDefinition
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "branches")]
        public List<BranchDefinition> Branches { get; set; }

        public BranchDefinition FindMatch(string branch)
        {
            var result = Branches.FirstOrDefault(x => x.IsMatch(branch));
            if (result is null)
            {
                return Branches.Last();
            }

            return result;
        }
    }

    public class BranchDefinition
    {
        [YamlDotNet.Serialization.YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "regex")]
        public string Regex { get; set; }
        [YamlDotNet.Serialization.YamlMember(Alias = "deployment-repo")]
        public string DeploymentRepo { get; set; }
        [YamlDotNet.Serialization.YamlMember(Alias = "auto-deploy")]
        public bool AutoDeploy { get; set; }
        [YamlDotNet.Serialization.YamlMember(Alias = "deploy-name")]
        public string DeployName { get; set; }
        [YamlDotNet.Serialization.YamlMember(Alias = "domain")]
        public string Domain { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "require_approval")]
        public bool RequireApproval { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "app_version")]
        public string AppVersion { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "dns_zone")]
        public string DnsZone { get; set; }
        [YamlDotNet.Serialization.YamlMember(Alias = "monolith_version")]
        public string MonolithVersion { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "modules_path")]
        public string modules_path { get; set; }

        [YamlDotNet.Serialization.YamlMember(Alias = "deploy_path")]
        public string deploy_path { get; set; }


        [YamlDotNet.Serialization.YamlMember(Alias = "pr")]
        public string PR { get; set; }

        [YamlDotNet.Serialization.YamlIgnore()]
        public Octokit.PullRequest PullRequest { get; set; }

        public bool IsMatch(string branch)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(branch, this.Regex);
        }
    }

    public class Context
    {
        public BuildContext build { get; set; }
        public PRContext pr { get; set; }

        public string modules_path { get; set; }
        public DeploymentContext deployment { get; set; }
        public Context()
        {
            build = new BuildContext();
            pr = new PRContext();
            deployment = new DeploymentContext();
            modules_path = "../modules";
        }

    }
    public class DeploymentPayload
    {
        public string PR { get; set; }
        public string Ref { get; set; }
        public string Sha { get; set; }
        public string Version { get; set; }
    }
    public class DeploymentContext
    {
        public string monolith_version { get; set; }
        public string dns_zone { get; set; }
        public string app_version { get; set; }
        
        public string domain { get; set; }

        public string pr { get; set; }

        public DeploymentPayload Payload { get; set; }
    }
    public class BuildContext
    {
        public Version version { get; set; }
        

    }
    public class PRContext
    {
        public string number { get; set; }

    }
}
