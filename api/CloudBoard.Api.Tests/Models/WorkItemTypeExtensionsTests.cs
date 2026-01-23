using CloudBoard.Api.Models;
using CloudBoard.Api.Models.Extensions;
using FluentAssertions;

namespace CloudBoard.Api.Tests.Models;

/// <summary>
/// Tests for WorkItemType extension methods ensuring hierarchy rules are correct.
/// </summary>
public class WorkItemTypeExtensionsTests
{
    [Theory]
    [InlineData(WorkItemType.Epic, true)]
    [InlineData(WorkItemType.Feature, true)]
    [InlineData(WorkItemType.PBI, true)]
    [InlineData(WorkItemType.Task, false)]
    [InlineData(WorkItemType.Bug, false)]
    public void CanHaveChildren_ReturnsCorrectValue(WorkItemType type, bool expected)
    {
        type.CanHaveChildren().Should().Be(expected);
    }

    [Fact]
    public void GetAllowedChildTypes_Epic_ReturnsFeatureAndBug()
    {
        var allowed = WorkItemType.Epic.GetAllowedChildTypes();

        allowed.Should().Contain(WorkItemType.Feature);
        allowed.Should().Contain(WorkItemType.Bug);
        allowed.Should().NotContain(WorkItemType.PBI);
        allowed.Should().NotContain(WorkItemType.Task);
    }

    [Fact]
    public void GetAllowedChildTypes_Feature_ReturnsPBIAndBug()
    {
        var allowed = WorkItemType.Feature.GetAllowedChildTypes();

        allowed.Should().Contain(WorkItemType.PBI);
        allowed.Should().Contain(WorkItemType.Bug);
        allowed.Should().NotContain(WorkItemType.Task);
    }

    [Fact]
    public void GetAllowedChildTypes_PBI_ReturnsTaskAndBug()
    {
        var allowed = WorkItemType.PBI.GetAllowedChildTypes();

        allowed.Should().Contain(WorkItemType.Task);
        allowed.Should().Contain(WorkItemType.Bug);
    }

    [Fact]
    public void GetAllowedChildTypes_Task_ReturnsEmpty()
    {
        var allowed = WorkItemType.Task.GetAllowedChildTypes();
        allowed.Should().BeEmpty();
    }

    [Theory]
    [InlineData(WorkItemType.Epic, 0)]
    [InlineData(WorkItemType.Feature, 1)]
    [InlineData(WorkItemType.PBI, 2)]
    [InlineData(WorkItemType.Task, 3)]
    [InlineData(WorkItemType.Bug, -1)] // Flexible level
    public void GetHierarchyLevel_ReturnsCorrectLevel(WorkItemType type, int expectedLevel)
    {
        type.GetHierarchyLevel().Should().Be(expectedLevel);
    }

    [Fact]
    public void GetDisplayName_PBI_ReturnsFullName()
    {
        WorkItemType.PBI.GetDisplayName().Should().Be("Product Backlog Item");
    }

    [Fact]
    public void GetDisplayName_Task_ReturnsTypeName()
    {
        WorkItemType.Task.GetDisplayName().Should().Be("Task");
    }
}
