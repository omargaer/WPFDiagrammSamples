using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Diagramm1;

public partial class ViewModel
{
    // Флаг, указывающий, нажата ли мышь private bool _isDown = false;
    private bool _isDown = false;
    private readonly ObservableCollection<ObservablePoint> _values = new();

    public ViewModel()
    {
        var trend = 1000;
        var r = new Random();
        // Добавление данных для диаграммы
        for (var i = 0; i < 500; i++)
        {
            // Генерация случайных изменений для тренда
            _values.Add(new ObservablePoint(i, trend += r.Next(-40, 40)));
        }
        // Инициализация основной серии графика
        Series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = _values,
                GeometryStroke = null,
                GeometryFill = null,
                DataPadding = new(0, 1)
            }
        };
        // Инициализация серии для полосы прокрутки
        ScrollbarSeries = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = _values,
                GeometryStroke = null,
                GeometryFill = null,
                DataPadding = new(0, 1)
            }
        };
        // Настройка скроллируемых осей
        ScrollableAxes = new[] { new Axis() };
        // Добавление секции для выделения области на графике
        Thumbs = new[]
        {
            new RectangularSection
            {
                Fill = new SolidColorPaint(new SKColor(255, 205, 210, 100))
            }
        };
        // Скрытые оси X и Y для использования в скроллинге
        InvisibleX = new[] { new Axis { IsVisible = false } };
        InvisibleY = new[] { new Axis { IsVisible = false } };
        // Настройка отступов графика
        Margin = new(100, LiveChartsCore.Measure.Margin.Auto, 50, LiveChartsCore.Measure.Margin.Auto);
    }
    // Свойства для привязки данных к представлению
    public ISeries[] Series { get; set; }
    public Axis[] ScrollableAxes { get; set; }
    public ISeries[] ScrollbarSeries { get; set; }
    public Axis[] InvisibleX { get; set; }
    public Axis[] InvisibleY { get; set; }
    public LiveChartsCore.Measure.Margin Margin { get; set; }
    public RectangularSection[] Thumbs { get; set; }
    // Команда, вызываемая при обновлении графика
    [RelayCommand]
    public void ChartUpdated(ChartCommandArgs args)
    {
        var cartesianChart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
        var x = cartesianChart.XAxes.First();
        var thumb = Thumbs[0];
        thumb.Xi = x.MinLimit;
        thumb.Xj = x.MaxLimit;
    }
    // Команда, вызываемая при нажатии мыши на график
    [RelayCommand]
    public void PointerDown(PointerCommandArgs args)
    {
        _isDown = true;
    }
    // Команда, вызываемая при перемещении мыши над графиком
    [RelayCommand]
    public void PointerMove(PointerCommandArgs args)
    {
        if (!_isDown) return; // Если мышь не нажата, выйти
        var chart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
        var positionInData = chart.ScalePixelsToData(args.PointerPosition);
        var thumb = Thumbs[0];
        var currentRange = thumb.Xj - thumb.Xi;
        thumb.Xi = positionInData.X - currentRange / 2;
        thumb.Xj = positionInData.X + currentRange / 2;
        ScrollableAxes[0].MinLimit = thumb.Xi;
        ScrollableAxes[0].MaxLimit = thumb.Xj;
    }
    // Команда, вызываемая при отпускании мыши над графиком
    [RelayCommand]
    public void PointerUp(PointerCommandArgs args)
    {
        _isDown = false;
    }
}