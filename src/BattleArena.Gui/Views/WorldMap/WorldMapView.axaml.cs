namespace BattleArena.Gui.Views.WorldMap;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Models.WorldMap;
using ViewModels.WorldMap;

public partial class WorldMapView : UserControl
{
    private const string MapPath = "Assets/WorldMap/aelthoria-world-map.png";

    public WorldMapView()
    {
        InitializeComponent();
        LoadMap();
    }

    private void LoadMap()
    {
        var path = System.IO.Path.Combine(AppContext.BaseDirectory, MapPath);
        if (System.IO.File.Exists(path))
            MapImage.Source = new Bitmap(path);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is WorldMapViewModel vm)
            RenderAll(vm);
    }

    private void RenderAll(WorldMapViewModel vm)
    {
        MarkerCanvas.Children.Clear();

        DrawRoads(vm);
        DrawMarkers(vm);
        DrawParty(vm);
    }

    private void DrawRoads(WorldMapViewModel vm)
    {
        foreach (var road in vm.Roads)
        {
            if (road.FromIndex >= vm.Locations.Count || road.ToIndex >= vm.Locations.Count)
                continue;

            var from = vm.Locations[road.FromIndex];
            var to = vm.Locations[road.ToIndex];

            var line = new Line
            {
                StartPoint = new Point(from.ScreenX, from.ScreenY),
                EndPoint = new Point(to.ScreenX, to.ScreenY),
                Stroke = new SolidColorBrush(Color.Parse("#88ffdd00")),
                StrokeThickness = 2,
                StrokeDashArray = [4, 4],
            };
            MarkerCanvas.Children.Add(line);
        }
    }

    private void DrawMarkers(WorldMapViewModel vm)
    {
        foreach (var loc in vm.Locations)
        {
            var color = loc.Type switch
            {
                LocationType.Capital => "#ff4444",
                LocationType.MajorCity => "#4488ff",
                LocationType.Town => "#44ddff",
                LocationType.Village => "#44dd44",
                LocationType.Fort => "#ff8844",
                LocationType.Port => "#44dddd",
                LocationType.Dungeon => "#ff44ff",
                LocationType.Cave => "#886644",
                LocationType.Ruins => "#aa66aa",
                LocationType.Encounter => "#ffaa00",
                _ => "#ffffff",
            };

            var size = loc.Type switch
            {
                LocationType.Capital => 16,
                LocationType.MajorCity => 13,
                LocationType.Town => 11,
                _ => 9,
            };

            // Marker circle
            var marker = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(Color.Parse(color)),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 2,
            };
            Canvas.SetLeft(marker, loc.ScreenX - size / 2.0);
            Canvas.SetTop(marker, loc.ScreenY - size / 2.0);
            MarkerCanvas.Children.Add(marker);

            // Name label
            var label = new TextBlock
            {
                Text = loc.Name,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 11,
                FontWeight = FontWeight.Bold,
            };
            Canvas.SetLeft(label, loc.ScreenX + size + 4);
            Canvas.SetTop(label, loc.ScreenY - 7);
            MarkerCanvas.Children.Add(label);
        }
    }

    private void DrawParty(WorldMapViewModel vm)
    {
        var party = new Ellipse
        {
            Width = 10,
            Height = 10,
            Fill = new SolidColorBrush(Color.Parse("#00e5ff")),
            Stroke = new SolidColorBrush(Colors.White),
            StrokeThickness = 2,
        };
        Canvas.SetLeft(party, vm.PartyX - 5);
        Canvas.SetTop(party, vm.PartyY - 5);
        MarkerCanvas.Children.Add(party);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (DataContext is not WorldMapViewModel vm) return;
        var pos = e.GetPosition(MapImage);

        foreach (var loc in vm.Locations)
        {
            var dx = pos.X - loc.ScreenX;
            var dy = pos.Y - loc.ScreenY;
            if (dx * dx + dy * dy < 15 * 15)
            {
                vm.SelectLocation(loc);
                StatusText.Text = $"{loc.Name} — {loc.Description}  [Enter to travel]";
                return;
            }
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (DataContext is not WorldMapViewModel vm) return;
        var pos = e.GetPosition(MapImage);

        foreach (var loc in vm.Locations)
        {
            var dx = pos.X - loc.ScreenX;
            var dy = pos.Y - loc.ScreenY;
            if (dx * dx + dy * dy < 15 * 15)
            {
                StatusText.Text = $"{loc.Name} — {loc.Description}  [Click to select]";
                return;
            }
        }

        StatusText.Text = vm.SelectedLocation is not null
            ? $"{vm.SelectedLocation.Name} selected — click Enter to travel"
            : "Click a location to select it";
    }
}
