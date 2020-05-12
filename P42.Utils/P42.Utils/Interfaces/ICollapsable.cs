using System;
namespace P42.Utils
{
    public interface ICollapsable
    {
        bool IsCollapsed { get; }
        bool IsTemplate { get; }
    }
}
