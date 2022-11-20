using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyPersonalWebApiInfra
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new MyPersonalWebApiInfraStack(app, "MyPersonalWebApiInfraStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                    Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION"),
                }
                // For more information, see https://docs.aws.amazon.com/cdk/latest/guide/environments.html
            });
            app.Synth();
        }
    }
}
