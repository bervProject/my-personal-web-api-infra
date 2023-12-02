using Amazon.CDK;
using Amazon.CDK.AWS.AppSync.Alpha;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Constructs;

namespace MyPersonalWebApiInfra
{
    public class MyPersonalWebApiInfraStack : Stack
    {
        internal MyPersonalWebApiInfraStack(Construct scope, string id, IStackProps props = null) : base(scope, id,
            props)
        {
            // AppSync
            var api = new GraphqlApi(this, "api", new GraphqlApiProps
            {
                Name = "berv-api",
                Schema = SchemaFile.FromAsset("schema.graphql"),
                XrayEnabled = true,
            });

            // Dynamo DB
            var experienceTable = new Table(this, "berv-experiences", new TableProps
            {
                BillingMode = BillingMode.PAY_PER_REQUEST,
                PartitionKey = new Attribute
                {
                    Name = "id",
                    Type = AttributeType.STRING
                },
            });

            // IAM Role
            var role = new Role(this, "Role", new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
            });

            // AppSync apply IAM to some mutations
            api.GrantMutation(role, "addExperience");
            api.GrantMutation(role, "updateExperience");
            api.GrantMutation(role, "deleteExperience");

            // Add Dynamo DB as DataSource of AppSync
            var dataSource = api.AddDynamoDbDataSource("experience", experienceTable);
            
            // Add Resolver for Get All Experiences
            dataSource.CreateResolver("get-experiences", new BaseResolverProps
            {
                TypeName = "Query",
                FieldName = "getExperiences",
                RequestMappingTemplate = MappingTemplate.DynamoDbScanTable(),
                ResponseMappingTemplate = MappingTemplate.DynamoDbResultList(),
            });

            // Add Resolver for Get Experience by Id
            dataSource.CreateResolver("get-experience-by-id", new BaseResolverProps
            {
                TypeName = "Query",
                FieldName = "getExperienceById",
                RequestMappingTemplate = MappingTemplate.DynamoDbGetItem("id", "experienceId"),
                ResponseMappingTemplate = MappingTemplate.DynamoDbResultItem(),
            });

            // Add Resolver for Create Experience
            dataSource.CreateResolver("add-experience", new BaseResolverProps
            {
                TypeName = "Mutation",
                FieldName = "addExperience",
                RequestMappingTemplate =
                    MappingTemplate.DynamoDbPutItem(PrimaryKey.Partition("id").Auto(), Values.Projecting("input")),
                ResponseMappingTemplate = MappingTemplate.DynamoDbResultItem()
            });

            // Add Resolver for Update an Experience
            dataSource.CreateResolver("update-experience",new BaseResolverProps
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

            // Add Resolver for Delete an Experience
            dataSource.CreateResolver("delete-experience", new BaseResolverProps
            {
                TypeName = "Mutation",
                FieldName = "deleteExperience",
                RequestMappingTemplate = MappingTemplate.DynamoDbDeleteItem("id", "experienceId"),
                ResponseMappingTemplate = MappingTemplate.FromString("$util.toJson($ctx.result)")
            });
        }
    }
}