using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Octokit;
using Version = System.Version;

namespace deploy
{
    class Program
    {
      

        static async Task Main(string[] args)
        {
            var yamlParser = new global::YamlDotNet.Serialization.Deserializer();
            DeploymentPayload payload;
            var data = System.Environment.GetEnvironmentVariable("INPUT_PAYLOAD");
            data =
                "\"{\\n  \\\"pr\\\": \\\"\\\",\\n  \\\"ref\\\": \\\"dev\\\",\\n  \\\"sha\\\": \\\"182ee80df33071b1908139c739f51f749e101c91\\\",\\n  \\\"version\\\": \\\"0.109.23897.10\\\"\\n}\"";
            if (data.StartsWith('"'))
            {
                data = data.Remove(0,1);
            }

            if (data.EndsWith('"'))
            {
                data = data.Remove(data.Length - 1,1);
            }
            Console.WriteLine(data);
            payload = Newtonsoft.Json.JsonConvert.DeserializeObject<DeploymentPayload>(data); 

            var branch = payload.Ref;


            //string path = @"C:\Source\billpay-deployment-qa";
            var workingDir = System.Environment.GetEnvironmentVariable("INPUT_WORKINGDIR");
            if (string.IsNullOrEmpty(workingDir))
            {
                workingDir = System.IO.Directory.GetCurrentDirectory();
            }
            string path = workingDir;
            Console.WriteLine("Working Directory: " + path);
            System.IO.Directory.SetCurrentDirectory(path);
            path = System.IO.Directory.GetCurrentDirectory();
            Console.WriteLine("Working Directory: " + path);
            var yaml = System.IO.File.ReadAllText(System.IO.Path.Combine(path, @".github/ci/branches.yaml"));
            var c = new Context();

            c.deployment.Payload = payload;
            c.pr.number = payload.PR;
            c.build.version = Version.Parse(payload.Version);


            var result = global::Stubble.Core.StaticStubbleRenderer.Render(yaml, c);
            var def = yamlParser.Deserialize<BranchesDefinition>(result);

            var branchDef = def.FindMatch(branch);

            c.deployment.pr = branchDef.PR;
            c.deployment.app_version = branchDef.AppVersion;
            c.deployment.dns_zone = branchDef.DnsZone;
            c.deployment.domain = branchDef.Domain;
            c.deployment.monolith_version = branchDef.MonolithVersion;
            c.modules_path = branchDef.modules_path;
            //load branch config 
            //if auto-deploy is true, then deploy -- otherwise don't 


            string terraform_path = branchDef.deploy_path;
            //ensure deployPath exists 
            System.IO.Directory.CreateDirectory(terraform_path);
            string template_path = @".github/deploy/template";

            foreach (var file in System.IO.Directory.EnumerateFiles(template_path))
            {
                System.IO.File.Copy(file, System.IO.Path.Combine(terraform_path, System.IO.Path.GetFileName(file)),
                    true);
            }

            //update auto.tfvars 
            Render(terraform_path, "app.auto.tfvars", c);
            Render(terraform_path, "main.tf", c);
            var version = Version.Parse(c.deployment.Payload.Version);
            var branchName = "v" + version.Build;
//            if (branchDef.AutoDeploy)
            {
                if (branchDef.RequireApproval)
                {
                    branchName = "approve/" + "v" + version.Build;
                }
            }
            Console.WriteLine("::set-output name=branch_name::" + branchName);
        }


        static void Render(string terraform_path, string path, Context c)
        {
            var filePath  = System.IO.Path.Combine(terraform_path, path);
            var template  = System.IO.File.ReadAllText(filePath);
            var rendered = Stubble.Core.StaticStubbleRenderer.Render(template, c);
            System.IO.File.WriteAllText(filePath, rendered);
        }
    }
}
    

