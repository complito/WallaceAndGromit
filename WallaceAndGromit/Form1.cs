using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WallaceAndGromit
{
    public enum AnimationDirection
    {
        Left,
        Right,
        Up,
        Down,
        None
    }

    public enum LocationName
    {
        Initial,
        Survival,
        Search,
        Rescue,
        None
    }

    public partial class Form1 : Form
    {
        private const int labelRange = 50;
        private bool isPressedAnyKey = false;
        private bool toUpdateAnimation = false;
        private LocationName currentLocation = LocationName.Initial;
        private LocationName nextLocation = LocationName.None;
        private Point cameraOffset = new Point(0, 0);
        private Player wallace;
        private Map map;
        private Label label = new Label
        {
            Size = new Size(150, 20),
            Location = new Point(50, 50),
            Text = "Press E to change location",
            Visible = false
        };
        private string partPathImage = "D:\\УрФУ\\ЯТП\\WallaceAndGromit\\WallaceAndGromit\\images\\";
        private string extension = ".png";
        private Timer timerAnimation = new Timer { Interval = 100 };
        private Timer timerMovement = new Timer { Interval = 10 };

        public Form1()
        {
            InitializeComponent();
            Controls.Add(label);
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            wallace = new Player(new Size(63, 100), 577, 310, 6);
            var mapLayout = new int[,] // 0 - grass, 1 - wall
            {
                { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1}
            };
            var grassImage = new Bitmap($"{partPathImage}Grass{extension}");
            var wallImage = new Bitmap($"{partPathImage}Wall{extension}");
            map = new Map(mapLayout, 64, 64, grassImage, wallImage);

            timerAnimation.Tick += new EventHandler(UpdateAnimation);
            timerAnimation.Start();
            timerMovement.Tick += new EventHandler(UpdateMovement);
            timerMovement.Start();

            KeyDown += new KeyEventHandler(Keybord);
            KeyUp += new KeyEventHandler(FreeKey);
            Paint += new PaintEventHandler(OnPaint);
        }

        private void UpdateAnimation(object sender, EventArgs e)
        {
            toUpdateAnimation = true;
        }

        private void UpdateMovement(object sender, EventArgs e)
        {
            ChangeLabelVisible();
            switch (wallace.CurrentAnimation)
            {
                case AnimationDirection.Left:
                    if (wallace.X > map.TextureWidth &&
                        (currentLocation == LocationName.Initial || currentLocation == LocationName.Survival))
                    {
                        wallace.Left();
                        //cameraOffset.X += wallace.speed;
                    }
                    break;
                case AnimationDirection.Right:
                    if (wallace.X + wallace.Size.Width < map.TextureWidth * (map.MapLayout.GetLength(0) - 1) &&
                        (currentLocation == LocationName.Initial || currentLocation == LocationName.Survival))
                    {
                        wallace.Right();
                        //cameraOffset.X -= wallace.speed;
                    }
                    break;
                case AnimationDirection.Up:
                    if (wallace.Y > map.TextureWidth &&
                        (currentLocation == LocationName.Initial || currentLocation == LocationName.Survival))
                    {
                        wallace.Up();
                        if (currentLocation == LocationName.Survival &&
                            wallace.Y >= map.TextureHeight * 4 &&
                            wallace.Y + wallace.Size.Height <= map.TextureHeight * (map.MapLayout.GetLength(1) - 5) - 5)
                            cameraOffset.Y += wallace.Speed;
                    }
                    break;
                case AnimationDirection.Down:
                    if (wallace.Y + wallace.Size.Height < map.TextureHeight * (map.MapLayout.GetLength(1) - 1) &&
                        currentLocation == LocationName.Initial)
                        wallace.Down();
                    else if (wallace.Y + wallace.Size.Height < map.TextureHeight * (map.MapLayout.GetLength(1) - 1) - 5 &&
                        currentLocation == LocationName.Survival)
                    {
                        wallace.Down();
                        if (wallace.Y >= map.TextureHeight * 4 &&
                            wallace.Y + wallace.Size.Height <= map.TextureHeight * (map.MapLayout.GetLength(1) - 5) - 5)
                            cameraOffset.Y -= wallace.Speed;
                    }
                    break;
                default: return;
            }
            Invalidate();
        }

        private void Keybord(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode.ToString())
            {
                case "A":
                    wallace.CurrentAnimation = AnimationDirection.Left;
                    break;
                case "D":
                    wallace.CurrentAnimation = AnimationDirection.Right;
                    break;
                case "W":
                    wallace.CurrentAnimation = AnimationDirection.Up;
                    break;
                case "S":
                    wallace.CurrentAnimation = AnimationDirection.Down;
                    break;
                case "E":
                    if (label.Visible && nextLocation != LocationName.None) ChangeLocation();
                    break;
                default:
                    return;
            }
            isPressedAnyKey = true;
        }

        private void FreeKey(object sender, KeyEventArgs e)
        {
            isPressedAnyKey = false;
            wallace.CurrentFrame = 0;
            if (wallace.CurrentAnimation == AnimationDirection.Left
                || wallace.CurrentAnimation == AnimationDirection.Right)
                wallace.PreviousAnimation = wallace.CurrentAnimation;
            wallace.CurrentAnimation = AnimationDirection.None;
            Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics gr = e.Graphics;
            CreateMap(gr);
            PlayAnimationMovement(gr);
        }

        private void PlayAnimationMovement(Graphics gr)
        {
            if (isPressedAnyKey)
            {
                if (toUpdateAnimation)
                {
                    if (wallace.CurrentFrame == 3) wallace.CurrentFrame = -1;
                    ++wallace.CurrentFrame;
                    toUpdateAnimation = false;
                }
                switch (wallace.CurrentAnimation)
                {
                    case AnimationDirection.Left:
                        DrawImage(gr, "Left");
                        break;
                    case AnimationDirection.Right:
                        DrawImage(gr, "Right");
                        break;
                    case AnimationDirection.Up:
                        DrawLeftOrRight(gr);
                        break;
                    case AnimationDirection.Down:
                        DrawLeftOrRight(gr);
                        break;
                }
            }
            else DrawLeftOrRight(gr);
        }

        private void DrawLeftOrRight(Graphics gr)
        {
            if (wallace.PreviousAnimation == AnimationDirection.Left) DrawImage(gr, "Left");
            else DrawImage(gr, "Right");
        }

        private void DrawImage(Graphics gr, string direction)
        {
            var wallaceImage = new Bitmap($"{partPathImage}Wallace_{direction}_{wallace.CurrentFrame}{extension}");
            gr.DrawImage(wallaceImage, wallace.X + cameraOffset.X,
                wallace.Y + cameraOffset.Y, wallace.Size.Width, wallace.Size.Height);
        }

        private void CreateMap(Graphics gr)
        {
            for (var i = 0; i < map.MapLayout.GetLength(0); ++i)
                for (var j = 0; j < map.MapLayout.GetLength(1); ++j)
                    switch (map.MapLayout[i, j])
                    {
                        case 0: // grass
                            gr.DrawImage(map.GrassImage, i * map.TextureWidth + cameraOffset.X,
                                j * map.TextureWidth + cameraOffset.Y, map.TextureWidth, map.TextureHeight);
                            break;
                        case 1: // wall
                            gr.DrawImage(map.WallImage, i * map.TextureWidth + cameraOffset.X,
                                j * map.TextureWidth + cameraOffset.Y, map.TextureWidth, map.TextureHeight);
                            break;
                    }
        }

        private void ChangeLabelVisible()
        {
            if (currentLocation == LocationName.Initial)
            {
                if (wallace.Y + wallace.Size.Height / 2 > map.TextureHeight * 4 && // left
                    wallace.Y + wallace.Size.Height / 2 < map.TextureHeight * 6 &&
                    wallace.X < map.TextureWidth + labelRange)
                    nextLocation = LocationName.Survival;
                else if (wallace.Y + wallace.Size.Height / 2 > map.TextureHeight * 4 && // right
                    wallace.Y + wallace.Size.Height / 2 < map.TextureHeight * 6 &&
                    wallace.X + wallace.Size.Width > Width - map.TextureWidth - labelRange)
                    nextLocation = LocationName.Rescue;
                else if (wallace.Y < map.TextureHeight + labelRange && // up
                    wallace.X + wallace.Size.Width / 2 > map.TextureWidth * 9 &&
                    wallace.X + wallace.Size.Width / 2 < map.TextureWidth * 11)
                    nextLocation = LocationName.Search;
                else if (wallace.Y + wallace.Size.Height > map.TextureHeight * (map.MapLayout.GetLength(1) - 1) - labelRange && // down
                    wallace.X + wallace.Size.Width / 2 > map.TextureWidth * 9 &&
                    wallace.X + wallace.Size.Width / 2 < map.TextureWidth * 11)
                    nextLocation = LocationName.None;
                else
                {
                    label.Visible = false;
                    nextLocation = LocationName.None;
                    return;
                }
                label.Visible = true;
            }
            else if (currentLocation == LocationName.Survival)
            {
                if (wallace.Y + wallace.Size.Height / 2 > map.TextureHeight * 4 && // right
                    wallace.Y + wallace.Size.Height / 2 < map.TextureHeight * 6 &&
                    wallace.X + wallace.Size.Width > Width - map.TextureWidth - labelRange)
                    nextLocation = LocationName.Initial;
                else if (wallace.Y + wallace.Size.Height > map.TextureHeight * (map.MapLayout.GetLength(1) - 1) - labelRange && // down
                    wallace.X + wallace.Size.Width / 2 > map.TextureWidth * 9 &&
                    wallace.X + wallace.Size.Width / 2 < map.TextureWidth * 11)
                    nextLocation = LocationName.Search;
                else
                {
                    label.Visible = false;
                    nextLocation = LocationName.None;
                    return;
                }
                label.Visible = true;
            }
        }

        private void ChangeLocation()
        {
            switch (nextLocation)
            {
                case LocationName.Initial:
                    map.MapLayout = new int[,] // 0 - grass, 1 - wall
                    {
                        { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1}
                    };
                    switch (currentLocation)
                    {
                        case LocationName.Survival:
                            wallace.X = map.TextureWidth + wallace.Size.Width / 2;
                            wallace.Y = map.TextureHeight * 4;
                            break;
                    }
                    currentLocation = LocationName.Initial;
                    cameraOffset = new Point(0, 0);
                    break;
                case LocationName.Survival:
                    map.MapLayout = new int[,] // 0 - grass, 1 - wall
                    {
                        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
                    };
                    switch (currentLocation)
                    {
                        case LocationName.Initial:
                            wallace.X = Width - map.TextureWidth - labelRange - wallace.Size.Width / 2;
                            wallace.Y = map.TextureHeight * 4;
                            break;
                    }
                    currentLocation = LocationName.Survival;
                    break;
                case LocationName.Search:

                    break;
                case LocationName.Rescue:

                    break;
            }
        }
    }
}
