﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PluginInterface;

namespace TETRIS_DS
{
    public static class SLZ
    {
        public static void Read(string file, int id, IPluginHost pluginHost)
        {
            pluginHost.Descomprimir(file);
            string dec_file;
            Carpeta dec_folder = pluginHost.Get_Files();

            if (dec_folder.files is List<Archivo>)
                dec_file = dec_folder.files[0].path;
            else
            {
                string tempFile = Path.GetTempFileName();
                Byte[] compressFile = new Byte[(new FileInfo(file).Length) - 0x08];
                Array.Copy(File.ReadAllBytes(file), 0x08, compressFile, 0, compressFile.Length); ;
                File.WriteAllBytes(tempFile, compressFile);

                pluginHost.Descomprimir(tempFile);
                dec_file = pluginHost.Get_Files().files[0].path;
            }

            uint file_size = (uint)new FileInfo(dec_file).Length;
            BinaryReader br = new BinaryReader(File.OpenRead(dec_file));			

            NSCR nscr = new NSCR();
            nscr.id = (uint)id;

            nscr.cabecera.id = "SLZ ".ToCharArray();
            nscr.cabecera.endianess = 0xFEFF;
            nscr.cabecera.constant = 0x0100;
            nscr.cabecera.file_size = file_size;
            nscr.cabecera.header_size = 0x10;
            nscr.cabecera.nSection = 1;

            nscr.section.id = "SLZ ".ToCharArray();
            nscr.section.section_size = file_size;
            nscr.section.width = 0x0100;
            nscr.section.height = 0x0100;
            nscr.section.padding = 0x00000000;
            nscr.section.data_size = file_size;
            nscr.section.mapData = new NTFS[file_size / 2];

            for (int i = 0; i < (file_size / 2); i++)
            {
                string bits = pluginHost.BytesToBits(br.ReadBytes(2));

                nscr.section.mapData[i] = new NTFS();
                nscr.section.mapData[i].nPalette = Convert.ToByte(bits.Substring(0, 4), 2);
                nscr.section.mapData[i].yFlip = Convert.ToByte(bits.Substring(4, 1), 2);
                nscr.section.mapData[i].xFlip = Convert.ToByte(bits.Substring(5, 1), 2);
                nscr.section.mapData[i].nTile = Convert.ToUInt16(bits.Substring(6, 10), 2);
            }

            br.Close();
            pluginHost.Set_NSCR(nscr);
        }
    }
}
