global using Xunit;
using Xunit.Priority;
[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCollectionOrderer("WHMapper.Tests.DisplayNameOrderer", "WHMapper.Tests")]
[assembly: TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]

