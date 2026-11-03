using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace Stil.Blazor.Components;

/// <summary>
/// A data grid column bound to a property. The Property expression is used to show, sort and
/// filter the column, also when the grid's data is an IQueryable (for example EF Core).
/// </summary>
public sealed class StilDataGridColumn<TItem, TValue> : StilDataGridColumnBase<TItem>
{
    private Expression<Func<TItem, TValue>>? _compiledFor;
    private Func<TItem, TValue>? _getter;

    /// <summary>The property, for example <c>o =&gt; o.Customer</c>.</summary>
    [Parameter, EditorRequired]
    public Expression<Func<TItem, TValue>> Property { get; set; } = default!;

    /// <summary>A format string for the value, for example "N2" or "d MMM yyyy".</summary>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>Custom cell content (for example a badge). Sorting and filtering still use Property.</summary>
    [Parameter]
    public RenderFragment<TItem>? Template { get; set; }

    /// <summary>Sortable by clicking the header. The default is true.</summary>
    [Parameter]
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// Shows a filter for the column. The default follows the grid's Filterable. Text columns get
    /// a "contains" filter; enum and bool columns a choice. Other types cannot be filtered.
    /// </summary>
    [Parameter]
    public bool? Filterable { get; set; }

    /// <summary>Lets the rows be grouped by this column (StilDataGrid.GroupBy). The default is true.</summary>
    [Parameter]
    public bool Groupable { get; set; } = true;

    /// <summary>Sort the grid by this column when it first loads.</summary>
    [Parameter]
    public StilSortDirection InitialSort { get; set; }

    /// <summary>The text for true in the cells and the filter of a bool column. The default is the localized "Yes".</summary>
    [Parameter]
    public string? TrueText { get; set; }

    /// <summary>The text for false in the cells and the filter of a bool column. The default is the localized "No".</summary>
    [Parameter]
    public string? FalseText { get; set; }

    private string TrueDisplay => Grid?.Localize(TrueText, "DataGrid.Yes") ?? TrueText ?? "Yes";

    private string FalseDisplay => Grid?.Localize(FalseText, "DataGrid.No") ?? FalseText ?? "No";

    private static Type ValueType => Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

    private string? PropertyName => (Property?.Body as MemberExpression)?.Member.Name;

    internal override string DisplayTitle => Title ?? PropertyName ?? "";

    internal override string ColumnKey => Key ?? PropertyName ?? Title ?? "";

    internal override bool CanSort => Sortable;

    internal override bool CanFilter =>
        (Filterable ?? Grid?.Filterable ?? false)
        && (ValueType == typeof(string) || ValueType == typeof(bool) || ValueType.IsEnum);

    internal override StilSortDirection InitialSortDirection => InitialSort;

    internal override bool CanGroup => Groupable;

    internal override IReadOnlyList<object?> GroupValues(IQueryable<TItem> query, bool descending)
    {
        var values = query.Select(Property).Distinct();
        var self = Expression.Parameter(typeof(TValue), "v");
        var identity = Expression.Lambda<Func<TValue, TValue>>(self, self);
        return (descending ? values.OrderByDescending(identity) : values.OrderBy(identity)).AsEnumerable().Cast<object?>().ToList();
    }

    internal override IQueryable<TItem> WhereGroup(IQueryable<TItem> query, object? value) =>
        query.Where(Expression.Lambda<Func<TItem, bool>>(
            Expression.Equal(Property.Body, Expression.Constant(value, typeof(TValue))), Property.Parameters));

    internal override string FormatGroupValue(object? value) => value switch
    {
        null => Grid?.Localize(null, "DataGrid.Blank") ?? "(Blank)",
        bool flag => flag ? TrueDisplay : FalseDisplay,
        IFormattable formattable when Format is not null => formattable.ToString(Format, CultureInfo.CurrentCulture),
        _ => value.ToString() ?? ""
    };

    internal override IReadOnlyList<(string Value, string Text)>? FilterOptions
    {
        get
        {
            if (ValueType == typeof(bool))
            {
                return [("true", TrueDisplay), ("false", FalseDisplay)];
            }

            return ValueType.IsEnum
                ? Enum.GetNames(ValueType).Select(name => (name, name)).ToList()
                : null;
        }
    }

    protected override void OnParametersSet()
    {
        if (!ReferenceEquals(_compiledFor, Property))
        {
            _getter = Property.Compile();
            _compiledFor = Property;
        }

        base.OnParametersSet();
    }

    internal override RenderFragment RenderCell(TItem item)
    {
        if (Template is not null)
        {
            return Template(item);
        }

        var value = _getter!(item);
        return builder => builder.AddContent(0, value switch
        {
            bool flag => flag ? TrueDisplay : FalseDisplay,
            IFormattable formattable when Format is not null => formattable.ToString(Format, CultureInfo.CurrentCulture),
            _ => value?.ToString()
        });
    }

    internal override string? ValueFormat => Format;

    internal override bool CanExport => Exportable;

    internal override string ExportText(TItem item) => ExportValue is not null
        ? StilDataGridExport.Text(ExportValue(item), Format, null)
        : StilDataGridExport.Text(_getter!(item), Format, value => value ? TrueDisplay : FalseDisplay);

    internal override object? ComputeAggregate(IQueryable<TItem> query)
    {
        switch (Aggregate)
        {
            case StilAggregate.Count:
                return query.Count();
            case StilAggregate.Min or StilAggregate.Max:
                // Min and Max of no rows are not defined; show nothing.
                if (!query.Any())
                {
                    return null;
                }

                return Aggregate == StilAggregate.Min ? query.Min(Property) : query.Max(Property);
            case StilAggregate.Sum or StilAggregate.Average:
                // Through decimal? or double?, which LINQ and EF Core sum for every number type.
                var floating = ValueType == typeof(double) || ValueType == typeof(float);
                var target = floating ? typeof(double?) : typeof(decimal?);
                var selector = Expression.Lambda(Expression.Convert(Property.Body, target), Property.Parameters);
                var method = typeof(Queryable).GetMethods()
                    .Single(m => m.Name == (Aggregate == StilAggregate.Sum ? nameof(Queryable.Sum) : nameof(Queryable.Average))
                        && m.IsGenericMethodDefinition
                        && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments()[1] == target)
                    .MakeGenericMethod(typeof(TItem));
                return method.Invoke(null, [query, selector]);
            default:
                return null;
        }
    }

    internal override IQueryable<TItem> ApplySort(IQueryable<TItem> query, bool descending, bool thenBy = false) => thenBy
        ? (descending ? ((IOrderedQueryable<TItem>)query).ThenByDescending(Property) : ((IOrderedQueryable<TItem>)query).ThenBy(Property))
        : (descending ? query.OrderByDescending(Property) : query.OrderBy(Property));

    internal override IQueryable<TItem> ApplyFilter(IQueryable<TItem> query, string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !CanFilter)
        {
            return query;
        }

        var parameter = Property.Parameters[0];
        var body = Property.Body;
        Expression condition;

        if (ValueType == typeof(string))
        {
            // x.Prop != null && x.Prop.ToLower().Contains(value.ToLower()): case-insensitive in
            // memory, and translated to LOWER(...) LIKE by EF Core.
            var toLower = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
            var contains = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
            condition = Expression.AndAlso(
                Expression.NotEqual(body, Expression.Constant(null, typeof(string))),
                Expression.Call(Expression.Call(body, toLower), contains, Expression.Constant(value.Trim().ToLowerInvariant())));
        }
        else
        {
            object? parsed;
            try
            {
                parsed = ValueType == typeof(bool) ? bool.Parse(value) : Enum.Parse(ValueType, value);
            }
            catch (Exception ex) when (ex is FormatException or ArgumentException)
            {
                return query;
            }

            condition = Expression.Equal(body, Expression.Constant(parsed, typeof(TValue)));
        }

        return query.Where(Expression.Lambda<Func<TItem, bool>>(condition, parameter));
    }
}
