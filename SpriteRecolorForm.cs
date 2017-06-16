﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MLLE
{
    public partial class SpriteRecolorForm : Form
    {
        bool Result = false;
        PaletteImage LevelPalette = new PaletteImage(8, 1, false, false);
        PaletteImage RemappingPalette = new PaletteImage(8, 1, false, false);
        ColorPalette ImageWorkingPalette;
        byte[] ImageIndices;
        byte[] ColorRemappings = new byte[Palette.PaletteSize];
        public SpriteRecolorForm()
        {
            InitializeComponent();
        }
        /*internal bool ShowForm(Palette palette, J2TFile tileset)
        {
            //CreateImageFromTileset(tileset);
            ShowDialog();
            return Result;
        }*/
        internal bool ShowForm(Palette palette, Bitmap bitmap, ref byte[] colorRemappings, Color imageBackgroundColor)
        {
            //CreateImageFromTileset(tileset);

            if (colorRemappings == null)
                for (uint i = 0; i < Palette.PaletteSize; ++i)
                    ColorRemappings[i] = (byte)i;
            else
                colorRemappings.CopyTo(ColorRemappings, 0);

            LevelPalette.Palette = palette;
            LevelPalette.Palette[0] = Palette.Convert(imageBackgroundColor);
            for (int i = 0; i < Palette.PaletteSize; ++i)
                RemappingPalette.Palette[i] = LevelPalette.Palette[ColorRemappings[i]];
            ImageWorkingPalette = bitmap.Palette;
            PaletteImage original = new PaletteImage(5, 0, true, false);
            for (uint i = 0; i < Palette.PaletteSize; ++i)
            {
                original.Palette.Colors[i] = Palette.Convert(ImageWorkingPalette.Entries[i]);
                ImageWorkingPalette.Entries[i] = Palette.Convert(LevelPalette.Palette.Colors[i]);
            }
            bitmap.Palette = ImageWorkingPalette;

            pictureBox1.Image = bitmap;
            pictureBox1.Size = bitmap.Size;
            Width -= (320 - pictureBox1.Width);

            original.Location = new Point(label1.Location.X, label1.Location.Y + 16);
            original.MouseMove += PaletteImageMouseMove;
            original.MouseLeave += PaletteImageMouseLeave;
            Controls.Add(original);

            LevelPalette.Location = new Point(label3.Left, panel1.Top);
            LevelPalette.MouseMove += PaletteImageMouseMove;
            LevelPalette.MouseLeave += PaletteImageMouseLeave;
            LevelPalette.MouseDown += PaletteImageMouseDown;
            Controls.Add(LevelPalette);

            RemappingPalette.Location = new Point(label4.Left, label4.Location.Y + 16);
            RemappingPalette.MouseMove += PaletteImageMouseMove;
            RemappingPalette.MouseLeave += PaletteImageMouseLeave;
            RemappingPalette.MouseDown += PaletteImageMouseDown;
            Controls.Add(RemappingPalette);

            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            ImageIndices = new byte[data.Width * data.Height];
            for (int y = 0; y < bitmap.Height; ++y)
                Marshal.Copy(new IntPtr((int)data.Scan0 + data.Stride * y), ImageIndices, bitmap.Width * y, bitmap.Width);
            bitmap.UnlockBits(data);

            for (uint i = 0; i < Palette.PaletteSize; ++i)
                original.ColorDisabled[i] = RemappingPalette.ColorDisabled[i] = i == 0 || !ImageIndices.Contains((byte)i);
            for (int i = 0; i < 10; ++i)
                LevelPalette.ColorDisabled[i] = LevelPalette.ColorDisabled[Palette.PaletteSize - 10 + i] = true; //transparency, and default windows colors

            var AllPaletteColors = Enumerable.Range(0, (int)Palette.PaletteSize).ToArray();
            original.Update(AllPaletteColors);
            LevelPalette.Update(AllPaletteColors);
            RemappingPalette.Update(AllPaletteColors);

            ShowDialog();

            if (Result)
                colorRemappings = ColorRemappings;
            return Result;
        }

        private void PaletteImageMouseDown(object sender, MouseEventArgs e)
        {
            (sender as PaletteImage).Clicked(sender, e);
        }

        private void PaletteImageMouseLeave(object sender, EventArgs e)
        {
            Text = "Remap Image Palette";
        }

        private void PaletteImageMouseMove(object sender, MouseEventArgs e)
        {
            Text = "Remap Image Palette \u2013 " + (sender as PaletteImage).getSelectedColor(e);
            (sender as PaletteImage).Moved(sender, e);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Point pictureOrigin = pictureBox1.AutoScrollOffset;
            Text = "Remap Image Palette \u2013 " + ImageIndices[(e.Location.X - pictureOrigin.X) + (e.Location.Y - pictureOrigin.Y) * pictureBox1.Width];

        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            Result = true;
            Dispose();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}
