using Amazon.CDK;
using Amazon.CDK.AWS.AppSync.Alpha;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Constructs;
using System.Collections.Generic;

namespace MyPersonalWebApiInfra
{
    public class MyPersonalWebApiInfraStack : Stack
    {
        internal MyPersonalWebApiInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id,
            props)
        {
            var api = new GraphqlApi(this, "api", new GraphqlApiProps
            {
                Name = "berv-api",
                Schema = Schema.FromAsset("schema.graphql"),
                XrayEnabled = true,
            });

            var experienceTable = new Table(this, "berv-experiences", new TableProps
            {
                BillingMode = BillingMode.PAY_PER_REQUEST,
                PartitionKey = new Attribute
                {
                    Name = "id",
                    Type = AttributeType.STRING
                },
            });

            var role = new Role(this, "Role", new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
            });

            api.GrantMutation(role, "addExperience");
            api.GrantMutation(role, "updateExperience");
            api.GrantMutation(role, "deleteExperience");

            var dataSource = api.AddDynamoDbDataSource("experience", experienceTable);
            dataSource.CreateResolver(new BaseResolverProps
            {
                TypeName = "Query",
                FieldName = "getExperiences",
                RequestMappingTemplate = MappingTemplate.DynamoDbScanTable(),
                ResponseMappingTemplate = MappingTemplate.DynamoDbResultList(),
            });

            dataSource.CreateResolver(new BaseResolverProps
            {
                TypeName = "Query",
                FieldName = "getExperienceById",
                RequestMappingTemplate = MappingTemplate.DynamoDbGetItem("id", "experienceId"),
                ResponseMappingTemplate = MappingTemplate.DynamoDbResultItem(),
            });

            dataSource.CreateResolver(new BaseResolverProps
            {
                TypeName = "Mutation",
                FieldName = "addExperience",
                RequestMappingTemplate =
                    MappingTemplate.DynamoDbPutItem(PrimaryKey.Partition("id").Auto(), Values.Projecting("input")),
                ResponseMappingTemplate = MappingTemplate.DynamoDbResultItem()
            });

            dataSource.CreateResolver(new BaseResolverProps
            {
                TypeName = "Mutation",
                FieldName = "updateExperience",
                RequestMappingTemplate = MappingTemplate.FromString("""
                { 
                    "version" : "2017-02-28",
                    "operation" : "PutItem",
                    "key": {
                        "id" : $util.dynamodb.toDynamoDBJson($ctx.args.input.id),
                    },
                    "attributeValues" : {
                        "title" : $util.dynamodb.toDynamoDBJson($ctx.args.input.title)
                    }
                }
                """),
                ResponseMappingTemplate = MappingTemplate.DynamoDbResultItem()
            });

            dataSource.CreateResolver(new BaseResolverProps
            {
                TypeName = "Mutation",
                FieldName = "deleteExperience",
                RequestMappingTemplate = MappingTemplate.DynamoDbDeleteItem("id",
                    Values.Attribute("experienceId").Is("experienceId").RenderVariables()),
                ResponseMappingTemplate = MappingTemplate.DynamoDbResultItem()
            });
        }
    }
}