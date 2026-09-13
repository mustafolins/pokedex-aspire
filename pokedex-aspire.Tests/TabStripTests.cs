using Bunit;
using pokedex_aspire.Web.Components.Shared;

namespace pokedex_aspire.Tests;

public class TabStripTests : BunitContext
{
    private static readonly TestTab[] Tabs = [TestTab.Scanner, TestTab.Entry];

    [Test]
    public void RendersSelectedTabAndKeepsEveryPanelMounted()
    {
        var component = RenderTabStrip(TestTab.Scanner);

        var tabList = component.Find("[role='tablist']");
        var tabs = component.FindAll("[role='tab']");
        var panels = component.FindAll("[role='tabpanel']");

        Assert.Multiple(() =>
        {
            Assert.That(tabList.GetAttribute("aria-label"), Is.EqualTo("Test views"));
            Assert.That(tabs, Has.Count.EqualTo(2));
            Assert.That(tabs[0].GetAttribute("aria-selected"), Is.EqualTo("true"));
            Assert.That(tabs[0].GetAttribute("aria-controls"), Is.EqualTo("test-tabs-panel-0"));
            Assert.That(tabs[1].GetAttribute("aria-selected"), Is.EqualTo("false"));
            Assert.That(panels, Has.Count.EqualTo(2));
            Assert.That(panels[0].HasAttribute("hidden"), Is.False);
            Assert.That(panels[0].TextContent, Does.Contain("Scanner panel"));
            Assert.That(panels[1].HasAttribute("hidden"), Is.True);
            Assert.That(panels[1].TextContent, Does.Contain("Entry panel"));
        });
    }

    [Test]
    public void ClickingTabSelectsItAndRaisesChange()
    {
        TestTab? selectedTab = null;
        var component = RenderTabStrip(
            TestTab.Scanner,
            selected => selectedTab = selected);

        component.Find("#test-tabs-tab-1").Click();

        var tabs = component.FindAll("[role='tab']");
        var panels = component.FindAll("[role='tabpanel']");

        Assert.Multiple(() =>
        {
            Assert.That(selectedTab, Is.EqualTo(TestTab.Entry));
            Assert.That(tabs[0].GetAttribute("aria-selected"), Is.EqualTo("false"));
            Assert.That(tabs[1].GetAttribute("aria-selected"), Is.EqualTo("true"));
            Assert.That(panels[0].HasAttribute("hidden"), Is.True);
            Assert.That(panels[1].HasAttribute("hidden"), Is.False);
        });
    }

    private IRenderedComponent<TabStrip<TestTab>> RenderTabStrip(
        TestTab selectedTab,
        Action<TestTab>? selectedTabChanged = null)
    {
        return Render<TabStrip<TestTab>>(parameters => parameters
            .Add(component => component.Id, "test-tabs")
            .Add(component => component.AriaLabel, "Test views")
            .Add(component => component.Items, Tabs)
            .Add(component => component.ItemText, GetTabText)
            .Add(component => component.SelectedItem, selectedTab)
            .Add(component => component.SelectedItemChanged, selectedTabChanged ?? (_ => { }))
            .Add(component => component.ChildContent, item => builder =>
                builder.AddContent(0, $"{GetTabText(item)} panel")));
    }

    private static string GetTabText(TestTab tab) => tab switch
    {
        TestTab.Scanner => "Scanner",
        TestTab.Entry => "Entry",
        _ => throw new ArgumentOutOfRangeException(nameof(tab), tab, null)
    };

    private enum TestTab
    {
        Scanner,
        Entry
    }
}