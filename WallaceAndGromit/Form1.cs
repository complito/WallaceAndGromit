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

    public partial class Form1 : Form
    {
        private bool isPressedAnyKey = false;
        private bool toUpdateAnimation = false;
        private Player wallace;
        private string partPathImage = "D:\\УрФУ\\ЯТП\\WallaceAndGromit\\WallaceAndGromit\\images\\";
        private string extension = ".png";
        private Timer timerAnimation = new Timer { Interval = 100 };
        private Timer timerMovement = new Timer { Interval = 10 };

        public Form1()
        {
            InitializeComponent();
            wallace = new Player(new Size(192, 304), 100, 100, 5);
            timerAnimation.Tick += new EventHandler(UpdateAnimation);
            timerAnimation.Start();
            timerMovement.Tick += new EventHandler(UpdateMovement);
            timerMovement.Start();
            KeyDown += new KeyEventHandler(Keybord);
            KeyUp += new KeyEventHandler(FreeKey);
            Paint += new PaintEventHandler(OnPaint);
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        private void UpdateAnimation(object sender, EventArgs e)
        {
            toUpdateAnimation = true;
        }

        private void UpdateMovement(object sender, EventArgs e)
        {
            switch (wallace.currentAnimation)
            {
                case AnimationDirection.Left:
                    wallace.Left();
                    break;
                case AnimationDirection.Right:
                    wallace.Right();
                    break;
                case AnimationDirection.Up:
                    wallace.Up();
                    break;
                case AnimationDirection.Down:
                    wallace.Down();
                    break;
                default:
                    return;
            }
            Invalidate();
        }

        private void Keybord(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode.ToString())
            {
                case "A":
                    wallace.currentAnimation = AnimationDirection.Left;
                    break;
                case "D":
                    wallace.currentAnimation = AnimationDirection.Right;
                    break;
                case "W":
                    wallace.currentAnimation = AnimationDirection.Up;
                    break;
                case "S":
                    wallace.currentAnimation = AnimationDirection.Down;
                    break;
                default:
                    return;
            }
            isPressedAnyKey = true;
        }

        private void FreeKey(object sender, KeyEventArgs e)
        {
            isPressedAnyKey = false;
            wallace.currentFrame = 0;
            if (wallace.currentAnimation == AnimationDirection.Left
                || wallace.currentAnimation == AnimationDirection.Right)
                wallace.previousAnimation = wallace.currentAnimation;
            wallace.currentAnimation = AnimationDirection.None;
            Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics gr = e.Graphics;
            PlayAnimationMovement(gr);
        }

        private void PlayAnimationMovement(Graphics gr)
        {
            if (isPressedAnyKey)
            {
                if (toUpdateAnimation)
                {
                    if (wallace.currentFrame == 3) wallace.currentFrame = -1;
                    ++wallace.currentFrame;
                    toUpdateAnimation = false;
                }
                switch (wallace.currentAnimation)
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
            if (wallace.previousAnimation == AnimationDirection.Left) DrawImage(gr, "Left");
            else DrawImage(gr, "Right");
        }

        private void DrawImage(Graphics gr, string direction)
        {
            gr.DrawImage(
                new Bitmap(
                    $"{partPathImage}Wallace_{direction}_{wallace.currentFrame}{extension}"),
                wallace.x, wallace.y, wallace.size.Width, wallace.size.Height);
        }
    }
}
