using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Spire.Xls;
using System.IO.Ports;
using System.Management;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Globalization;
using System.Reflection;
using System.Linq;
using System.Web.Script.Serialization;
using System.Net;
using ATS.MiniForm;
using ATS.Config;
using ATS.Hardware;
using ATS.Models;
using System.Text.RegularExpressions;
using System.IO.Pipes;
using Newtonsoft.Json;

namespace ATS
{
    public partial class fMain
    {
        #region ============================================================== EXCEL ===============================================================
        private bool CheckSameStep()
        {
            string nameSheetBefore = "";
            string nameSheetAfter = "";

            if (excel.workSheet.GetNumber(excel.info.rowStart, excel.info.column).ToString() == excel.nan)
            {
                nameSheetBefore = excel.workSheet.GetText(excel.info.rowStart, excel.info.column);

            }
            else
            {
                nameSheetBefore = excel.workSheet.GetNumber(excel.info.rowStart, excel.info.column).ToString();
            }


            for (int hh = 1; hh < configTester.numHead; hh++)
            {

                if (excel.workSheet.GetNumber(excel.info.rowStart + hh, excel.info.column).ToString() == excel.nan)
                {
                    nameSheetAfter = excel.workSheet.GetText(excel.info.rowStart + hh, excel.info.column);

                }
                else
                {
                    nameSheetAfter = excel.workSheet.GetNumber(excel.info.rowStart + hh, excel.info.column).ToString();
                }

                if (nameSheetBefore != nameSheetAfter)
                {
                    return false;
                }
            }


            if (excel.workSheet.GetNumber(excel.info.rowSequence, excel.info.column).ToString() == excel.nan)
            {
                nameSheetBefore = excel.workSheet.GetText(excel.info.rowSequence, excel.info.column);

            }
            else
            {
                nameSheetBefore = excel.workSheet.GetNumber(excel.info.rowSequence, excel.info.column).ToString();
            }


            for (int hh = 1; hh < configTester.numHead; hh++)
            {

                if (excel.workSheet.GetNumber(excel.info.rowSequence + hh, excel.info.column).ToString() == excel.nan)
                {
                    nameSheetAfter = excel.workSheet.GetText(excel.info.rowSequence + hh, excel.info.column);

                }
                else
                {
                    nameSheetAfter = excel.workSheet.GetNumber(excel.info.rowSequence + hh, excel.info.column).ToString();
                }

                if (nameSheetBefore != nameSheetAfter)
                {
                    return false;
                }
            }


            return true;
        }
        private void LoadDiscription()
        {
            excel.workSheet = excel.workBook.Worksheets[excel.info.nameSheet];

            //all process initial
            string initialFunction = excel.workSheet.GetText(excel.info.rowInitial, excel.info.column);
            if (initialFunction != null)
            {
                initialFunction = initialFunction.Trim();
                string[] initialFunctions = initialFunction.Replace("\r\n", "\n").Replace('\n', '$').Split('$');
                foreach (string function in initialFunctions)
                {
                    try
                    {
                        Activator.CreateInstance(functionExcel, this, function);
                    }
                    catch (Exception)
                    {
                        Log(LogMsgType.Error_Red, "\ncall function " + function + " error");
                    }
                }
            }


            this.Text = excel.workSheet.GetText(excel.info.rowCustomer, excel.info.column);

            if (excel.workSheet.GetNumber(excel.info.rowDetail, excel.info.column).ToString() == excel.nan)
            {
                tb_detail.Text = excel.workSheet.GetText(excel.info.rowDetail, excel.info.column) + "_" + cbb_fg.Text;

            }
            else
            {
                tb_detail.Text = excel.workSheet.GetText(excel.info.rowDetail, excel.info.column).ToString() + "_" + cbb_fg.Text; ;
            }

            if (excel.workSheet.GetNumber(excel.info.rowFirmware, excel.info.column).ToString() == excel.nan)
            {
                tb_fwVersion.Text = excel.workSheet.GetText(excel.info.rowFirmware, excel.info.column);

            }
            else
            {
                tb_fwVersion.Text = excel.workSheet.GetNumber(excel.info.rowFirmware, excel.info.column).ToString();
            }

            if (excel.workSheet.GetNumber(excel.info.rowSpecVersion, excel.info.column).ToString() == excel.nan)
            {
                tb_spec.Text = excel.workSheet.GetText(excel.info.rowSpecVersion, excel.info.column);

            }
            else
            {
                tb_spec.Text = excel.workSheet.GetNumber(excel.info.rowSpecVersion, excel.info.column).ToString();
            }

            excel.sameStep = CheckSameStep();

            for (int hh = 1; hh <= configTester.numHead; hh++)
            {
                ClearDataGridView(hh);
            }

            for (int hh = 1; hh <= configTester.numHead; hh++)
            {
                GetDescription(hh);
            }

            if (configTester.useRelayCard && !background_relay.IsBusy)
            {
                background_relay.RunWorkerAsync();
            }
        }
        private void GetDescription(int head)
        {
            DataGridView g = getDataGridView(head);

            excel.workSheet = excel.workBook.Worksheets[excel.info.nameSheet];
            int rowDatagrid = 0;
            int rowExcel = 1;
            excel.sheetTest = excel.workSheet.GetText((excel.info.rowStart + head) - 1, excel.info.column);

            try
            {
                excel.workSheet = excel.workBook.Worksheets[excel.sheetTest];
            }
            catch
            {
                MessageBox.Show(excel.info.errHead + head);
                return;
            }

            g.Rows.Add(GetRowExcel());
            excel.workSheet = excel.workBook.Worksheets[excel.sheetTest];

            string numberExcel;
            string detailExcel;
            string minExcel;
            string maxExcel;
            Color color;

            while (true)
            {
                rowExcel++;
                //string ggg = excel.workSheet.Range[rowExcel, 1].Style.KnownColor.ToString();

                if (excel.workSheet.Range[rowExcel, excel.pcba.columnNumber].Style.KnownColor.ToString() == excel.color.yellow ||
                    excel.workSheet.Range[rowExcel, excel.pcba.columnNumber].Style.KnownColor.ToString() == excel.color.red)
                {
                    continue;
                }

                numberExcel = excel.workSheet.GetText(rowExcel, excel.pcba.columnNumber);
                detailExcel = excel.workSheet.GetText(rowExcel, excel.pcba.columnDetail);
                minExcel = excel.workSheet.GetNumber(rowExcel, excel.pcba.columnMin).ToString();

                if (minExcel == excel.nan)
                {
                    minExcel = excel.workSheet.GetFormulaNumberValue(rowExcel, excel.pcba.columnMin).ToString();

                    if (minExcel == excel.nan)
                    {
                        minExcel = excel.workSheet.GetText(rowExcel, excel.pcba.columnMin);
                    }
                }

                if (minExcel == null)
                {
                    minExcel = excel.workSheet.GetFormulaStringValue(rowExcel, excel.pcba.columnMin);
                }

                if (minExcel != null)
                {
                    minExcel = minExcel.Trim();
                }

                maxExcel = excel.workSheet.GetNumber(rowExcel, excel.pcba.columnMax).ToString();

                if (maxExcel == excel.nan)
                {
                    maxExcel = excel.workSheet.GetFormulaNumberValue(rowExcel, excel.pcba.columnMax).ToString();

                    if (maxExcel == excel.nan)
                    {
                        maxExcel = excel.workSheet.GetText(rowExcel, excel.pcba.columnMax);
                    }
                }

                if (maxExcel == null)
                {
                    maxExcel = excel.workSheet.GetFormulaStringValue(rowExcel, excel.pcba.columnMax);
                }

                if (maxExcel != null)
                {
                    maxExcel = maxExcel.Trim();
                }

                if (numberExcel == null)
                {
                    numberExcel = excel.workSheet.GetNumber(rowExcel, excel.pcba.columnNumber).ToString();

                    if (numberExcel == excel.nan)
                    {
                        break;
                    }
                }

                string bbb = excel.workSheet.Range[rowExcel, excel.pcba.columnNumber].Style.KnownColor.ToString();
                if (excel.workSheet.Range[rowExcel, excel.pcba.columnNumber].Style.KnownColor.ToString() != excel.color.none &&
                    excel.workSheet.Range[rowExcel, excel.pcba.columnNumber].Style.KnownColor.ToString() != excel.color.skyBlue &&
                    excel.workSheet.Range[rowExcel, excel.pcba.columnNumber].Style.KnownColor.ToString() != excel.color.gold)
                {
                    color = excel.workSheet.Range[rowExcel, excel.pcba.columnNumber].Style.Color;
                    g.Rows[rowDatagrid].DefaultCellStyle.BackColor = Color.FromArgb(color.A, color.R, color.G, color.B);
                }

                g.Rows[rowDatagrid].Cells[0].Value = numberExcel;
                g.Rows[rowDatagrid].Cells[1].Value = detailExcel;

                if (minExcel != "" && minExcel != null)
                {
                    g.Rows[rowDatagrid].Cells[2].Value = minExcel;
                }

                if (minExcel != "" && minExcel != null && maxExcel != "" && maxExcel != null)
                {
                    g.Rows[rowDatagrid].Cells[2].Value = minExcel + " - " + maxExcel;
                }

                rowDatagrid++;
            }
        }
        private int GetRowExcel()
        {
            int rowDataGridView = 0;
            string getExcel;

            for (int i = 2; i < 9999; i++)
            {

                if (excel.workSheet.Range[i, 1].Style.KnownColor.ToString() == excel.color.yellow ||
                    excel.workSheet.Range[i, 1].Style.KnownColor.ToString() == excel.color.red)
                {
                    continue;
                }

                getExcel = excel.workSheet.GetText(i, excel.pcba.columnNumber);

                if (getExcel == null)
                {
                    getExcel = excel.workSheet.GetNumber(i, excel.pcba.columnNumber).ToString();

                    if (getExcel == excel.nan)
                    {
                        if (excel.workSheet.GetText(i, excel.pcba.columnDetail) != null)
                        {

                            MessageBox.Show(excel.pcba.errColumn);
                            rowDataGridView = 1;
                        }

                        break;
                    }
                }

                rowDataGridView += 1;
            }

            return rowDataGridView;
        }
        private int GetRowSequenceExcel()
        {
            int rowDataGridView = 0;
            string getExcel;

            for (int i = 1; i < 9999; i++)
            {
                getExcel = excel.workSheet.GetText(i, excel.pcba.columnNumber);

                if (getExcel == null)
                {
                    getExcel = excel.workSheet.GetNumber(i, excel.pcba.columnNumber).ToString();

                    if (getExcel == excel.nan)
                    {
                        break;
                    }
                }

                rowDataGridView += 1;
            }

            return rowDataGridView;
        }
        private int GetRowSteptestExcel()
        {
            int rowDataGridView = 0;
            string getExcel;

            for (int i = 2; i < 9999; i++)
            {

                if (excel.workSheet.Range[i, excel.pcba.columnNumber].Style.KnownColor.ToString() != excel.color.none &&
                    excel.workSheet.Range[i, excel.pcba.columnNumber].Style.KnownColor.ToString() != excel.color.skyBlue &&
                    excel.workSheet.Range[i, excel.pcba.columnNumber].Style.KnownColor.ToString() != excel.color.gold)
                {
                    continue;
                }

                getExcel = excel.workSheet.GetText(i, excel.pcba.columnNumber);

                if (getExcel == null)
                {
                    getExcel = excel.workSheet.GetNumber(i, excel.pcba.columnNumber).ToString();

                    if (getExcel == excel.nan)
                    {
                        break;
                    }
                }

                rowDataGridView += 1;
            }

            return rowDataGridView;
        }
        #endregion
    }
}
