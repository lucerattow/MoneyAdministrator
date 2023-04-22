﻿using MoneyAdministrator.Common.DTOs;
using MoneyAdministrator.Interfaces;
using MoneyAdministrator.Models;
using MoneyAdministrator.Utilities;
using MoneyAdministrator.Utilities.Disposable;
using System.Configuration;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MoneyAdministrator.Views
{
    public partial class TransactionHistoryView : UserControl, ITransactionHistoryView
    {
        //fields
        private int _selectedId = 0;
        private bool _isCreditCardSummaryOutstanding = false;

        private List<string> _frequencies = new List<string>()
        {
            "1 Mes",
            "3 Meses",
            "6 Meses",
            "12 Meses"
        };

        //grd columns width
        private const int _colWidthDate = 90;
        private const int _colWidthEntity = 210;
        private const int _colWidthInstall = 60;
        private const int _colWidthCurrency = 70;
        private const int _colWidthAmount = 120;
        private const int _colWidthTotal = _colWidthDate + _colWidthEntity + _colWidthInstall + _colWidthCurrency + _colWidthAmount;

        //properties
        public int SelectedId
        {
            get => _selectedId;
            set => _selectedId = value;
        }
        public string EntityName
        {
            get => _txtEntityName.Text;
            set => _txtEntityName.Text = value;
        }
        public DateTime Date
        {
            get => _dtpDate.Value;
            set => _dtpDate.Value = value;
        }
        public string Description
        {
            get => _txtDescription.Text;
            set => _txtDescription.Text = value;
        }
        public Currency Currency
        {
            get => (Currency)_cbCurrency.SelectedItem;
            set => _cbCurrency.SelectedIndex = _cbCurrency.FindStringExact(value.Name);
        }
        public decimal Amount
        {
            get
            {
                var numbers = new string(_txtAmount.Text.Where(char.IsDigit).ToArray());
                var value = decimal.Parse(numbers) / 100;

                var oper = _txtAmount.OperatorSymbol;
                if (oper == "-" && value > 0 || oper == "+" && value < 0)
                    value *= -1;

                return value;
            }
            set
            {
                if (value >= 0)
                    _txtAmount.OperatorSymbol = "+";
                else
                    _txtAmount.OperatorSymbol = "-";

                _txtAmount.Text = value.ToString("N2");
            }
        }

        public int InstallmentCurrent
        {
            get
            {
                var installments = string.Concat(_txtInstallmentCurrent.Text.Where(char.IsDigit));
                if (!string.IsNullOrEmpty(installments) && int.TryParse(installments, out int value))
                    return value;
                else
                    return 0;
            }
            set
            {
                if (value > 0)
                    _txtInstallmentCurrent.Text = value.ToString();
                else
                    _txtInstallmentCurrent.Text = "";
            }
        }
        public int InstallmentMax
        {
            get
            {
                var installments = _txtInstallments.Text;
                if (string.IsNullOrEmpty(installments) || installments == "0")
                    installments = "1";

                return int.Parse(installments);
            }
            set
            {
                if (value > 0)
                    _txtInstallments.Text = value.ToString();
                else
                    _txtInstallments.Text = "";
            }
        }
        public int Frequency
        {
            get
            {
                var frequency = string.Concat(_cbFrequency.SelectedItem.ToString().Where(char.IsDigit));

                if (!string.IsNullOrEmpty(frequency) && int.TryParse(frequency, out int value))
                    return value;
                else
                    return 0;
            }
            set
            {
                if (value < 1)
                    value = 1;
                _cbFrequency.SelectedIndex = _cbFrequency.FindString(value.ToString());
            }
        }

        public bool IsService
        {
            get => _ckbService.Checked;
            set => _ckbService.Checked = value;
        }
        public bool Editing
        {
            get => SelectedId > 0;
            set
            {
                _ckbService.Enabled = !value;
                if (InstallmentMax > 1)
                    _dtpDate.CustomFormat = "'Dia:' dd";
                else
                    _dtpDate.CustomFormat = ConfigurationManager.AppSettings["DateFormat"];
            }
        }
        public bool IsCreditCardSummaryOutstanding
        {
            get => _isCreditCardSummaryOutstanding;
            set
            {
                _isCreditCardSummaryOutstanding = value;
                ButtonsLogic();
            }
        }

        public TransactionHistoryView()
        {
            this.Visible = false;

            using (new CursorWait())
            {
                Dock = DockStyle.Fill;
                InitializeComponent();
                AssosiateEvents();
                ControlsSetup();
                ButtonsLogic();
            }

            //Muestro la ventana ya cargada
            this.Visible = true;
        }

        //methods
        public void SetCurrenciesList(List<Currency> datasource)
        {
            _cbCurrency.DataSource = datasource;
            _cbCurrency.DisplayMember = "Name";
        }

        public void GrdRefreshData(List<TransactionDto> datasource)
        {
            using (new CursorWait())
            using (new DataGridViewHide(_grd))
            {
                //Limpio la grilla y el yearPicker
                _grd.Rows.Clear();
                _ypYearPage.AvailableYears = datasource.Select(x => x.Date.Year).Distinct().ToList();

                //Filtro las transacciones por el año seleccionado
                datasource = datasource.Where(x => x.Date.Year == _ypYearPage.Value).ToList();

                if (datasource.Count <= 0)
                    return;

                var row = 0;
                for (var i = 12; i >= 1; i--)
                {
                    List<TransactionDto> monthTransactions = datasource
                        .Where(x => x.Date.Month == i).OrderByDescending(x => x.Date.Day).ThenBy(x => x.Description).ToList();

                    if (monthTransactions.Count != 0)
                    {
                        DateTime separatorDate = new DateTime(_ypYearPage.Value, i, 1);
                        //Añado un separador
                        row = _grd.Rows.Add(new object[]
                        {
                        -1,
                        separatorDate.ToString("yyyy"),
                        separatorDate.ToString("(MM) MMM"),
                        "",
                        "",
                        "",
                        "",
                        });

                        //Pinto el separador
                        Color separatorBackColor = Color.FromArgb(75, 135, 230);
                        Color separatorForeColor = Color.White;
                        PaintDgvCells.PaintSeparator(_grd, row, separatorBackColor, separatorForeColor);

                        //Caso contrario añado los registros a la tabla
                        foreach (var transaction in monthTransactions)
                        {
                            row = _grd.Rows.Add(new object[]
                            {
                            transaction.Id,
                            transaction.Date.ToString("yyyy-MM-dd"),
                            transaction.EntityName,
                            transaction.Description,
                            transaction.Installment,
                            transaction.CurrencyName,
                            transaction.Amount.ToString("#,##0.00 $", CultureInfo.GetCultureInfo("es-ES")),
                            });

                            //Pinto el monto segun corresponda
                            PaintDgvCells.PaintDecimal(_grd, row, "amount");
                        }
                    }
                }
            }
        }

        public void ButtonsLogic()
        {
            _tsbInsert.Enabled = _selectedId == 0;
            _tsbUpdate.Enabled = _selectedId != 0 && !_isCreditCardSummaryOutstanding;
            _tsbDelete.Enabled = _selectedId != 0;
            _tsbNewPay.Enabled = _selectedId != 0 && _isCreditCardSummaryOutstanding;
        }

        private void ClearInputs()
        {
            _selectedId = 0;
            _txtEntityName.Text = "";
            _txtDescription.Text = "";
            _dtpDate.Value = DateTime.Now;
            _txtAmount.Text = "0";
            _cbCurrency.SelectedIndex = _cbCurrency.FindStringExact("ARS");
            _txtInstallmentCurrent.Text = "";
            _txtInstallments.Text = "";
            _ckbService.Checked = false;
            _cbFrequency.SelectedIndex = _cbFrequency.FindString("1");

            Editing = false;
            _isCreditCardSummaryOutstanding = false;
            ButtonsLogic();
        }

        private void AssosiateEvents()
        {
            _btnEntitySearch.Click += (sender, e) => ButtonEntitySearchClick?.Invoke(sender, e);
            _tsbExit.Click += (sender, e) => ButtonExitClick?.Invoke(sender, e);
            _ypYearPage.ButtonNextClick += (sender, e) => SelectedYearChange?.Invoke(sender, e);
            _ypYearPage.ButtonPreviousClick += (sender, e) => SelectedYearChange?.Invoke(sender, e);
            _ypYearPage.ValueChange += (sender, e) => SelectedYearChange?.Invoke(sender, e);
        }

        private void ControlsSetup()
        {
            _txtEntityName.MaxLength = 25;
            _dtpDate.CustomFormat = ConfigurationManager.AppSettings["DateFormat"];
            _txtDescription.MaxLength = 150;
            _txtAmount.TextAlign = HorizontalAlignment.Right;
            _txtAmount.Text = "0";
            _txtInstallmentCurrent.MaxLength = 2;
            _txtInstallmentCurrent.TextAlign = HorizontalAlignment.Center;
            _txtInstallments.MaxLength = 2;
            _txtInstallments.TextAlign = HorizontalAlignment.Center;

            _cbFrequency.Enabled = false;
            _cbFrequency.DataSource = _frequencies;

            GrdSetup();
        }

        private void GrdSetup()
        {
            ControlConfig.DataGridViewSetup(_grd);

            //Configuracion de columnas
            _grd.Columns.Add(new DataGridViewColumn() //0 id
            {
                Name = "id",
                HeaderText = "Id",
                CellTemplate = new DataGridViewTextBoxCell(),
                Visible = false,
            });
            _grd.Columns.Add(new DataGridViewColumn() //1 Fecha
            {
                Name = "date",
                HeaderText = "Fecha",
                CellTemplate = new DataGridViewTextBoxCell(),
                Width = _colWidthDate,
                DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleLeft },
            });
            _grd.Columns.Add(new DataGridViewColumn() //2 entity
            {
                Name = "entity",
                HeaderText = "Entidad",
                CellTemplate = new DataGridViewTextBoxCell(),
                Width = _colWidthEntity,
            });
            _grd.Columns.Add(new DataGridViewColumn() //3 description
            {
                Name = "description",
                HeaderText = "Descripcion",
                CellTemplate = new DataGridViewTextBoxCell(),
            });
            _grd.Columns.Add(new DataGridViewColumn() //4 installments
            {
                Name = "installments",
                HeaderText = "Cuotas",
                CellTemplate = new DataGridViewTextBoxCell(),
                Width = _colWidthInstall,
                DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleCenter },
            });
            _grd.Columns.Add(new DataGridViewColumn() //5 currency
            {
                Name = "currency",
                HeaderText = "Moneda",
                CellTemplate = new DataGridViewTextBoxCell(),
                Width = _colWidthCurrency,
                DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleCenter },
            });
            _grd.Columns.Add(new DataGridViewColumn() //6 amount
            {
                Name = "amount",
                HeaderText = "Monto",
                CellTemplate = new DataGridViewTextBoxCell(),
                DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleRight },
                Width = _colWidthAmount
            });
        }

        //events
        private void _tsbInsert_Click(object sender, EventArgs e)
        {
            ButtonInsertClick?.Invoke(sender, e);
            ClearInputs();
        }

        private void _tsbNewPay_Click(object sender, EventArgs e)
        {
            ButtonNewPayClick.Invoke(sender, e);
            ButtonsLogic();
        }

        private void _tsbUpdate_Click(object sender, EventArgs e)
        {
            ButtonUpdateClick?.Invoke(sender, e);
            ClearInputs();
        }

        private void _tsbDelete_Click(object sender, EventArgs e)
        {
            ButtonDeleteClick?.Invoke(sender, e);
            ClearInputs();
        }

        private void _tsbClear_Click(object sender, EventArgs e)
        {
            ClearInputs();
        }

        private void _txtInstallmentCurrent_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void _ckbService_CheckedChanged(object sender, EventArgs e)
        {
            _cbFrequency.Enabled = _ckbService.Checked;

            _txtInstallmentCurrent.Enabled = !_ckbService.Checked;
            _txtInstallments.Enabled = !_ckbService.Checked;
        }

        private void _grd_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            _selectedId = (int)(sender as DataGridView).Rows[e.RowIndex].Cells[0].Value;
            GrdDoubleClick?.Invoke(sender, e);
            ButtonsLogic();
        }

        private void TransactionHistoryView_Resize(object sender, EventArgs e)
        {
            _grd.Columns["description"].Width = _grd.Width - _colWidthTotal - 19;
        }

        private void _txtInstallments_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar)))
                e.Handled = true;
        }

        public event EventHandler GrdDoubleClick;
        public event EventHandler ButtonInsertClick;
        public event EventHandler ButtonNewPayClick;
        public event EventHandler ButtonUpdateClick;
        public event EventHandler ButtonDeleteClick;
        public event EventHandler ButtonExitClick;
        public event EventHandler SelectedYearChange;
        public event EventHandler ButtonEntitySearchClick;
    }
}
