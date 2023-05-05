﻿using MoneyAdministrator.Common.DTOs;
using MoneyAdministrator.Common.Enums;
using MoneyAdministrator.Common.Utilities.TypeTools;
using MoneyAdministrator.CustomControls;
using MoneyAdministrator.Interfaces;
using MoneyAdministrator.Models;
using MoneyAdministrator.Utilities;
using MoneyAdministrator.Utilities.ControlTools;
using MoneyAdministrator.Utilities.Disposable;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Globalization;

namespace MoneyAdministrator.Views.UserControls
{
    public partial class TransactionHistoryView : UserControl, ITransactionHistoryView
    {
        //grd columns width
        private const int _colWidthDate = 90;
        private const int _colWidthEntity = 210;
        private const int _colWidthInstall = 60;
        private const int _colWidthCurrency = 70;
        private const int _colWidthAmount = 120;
        private const int _colCheckBox = 70;
        private const int _colWidthTotal = _colWidthDate + _colWidthEntity + _colWidthInstall + _colWidthCurrency + _colWidthAmount + (_colCheckBox * 2);

        //fields
        private TransactionViewDto? _selectedDto;
        private TransactionViewDto? _checkBoxChangeDto;
        private int _focusRow = 0;

        //properties
        public TransactionViewDto? SelectedDto
        {
            get => _selectedDto;
            set => _selectedDto = value;
        }
        public TransactionViewDto? CheckBoxChangeDto
        {
            get => _checkBoxChangeDto;
        }
        public int FocusRow
        {
            get => _focusRow;
            set => _focusRow = value;
        }

        //properties fields
        public string EntityName
        {
            get => _txtEntityName.Text;
            set => _txtEntityName.Text = value;
        }
        public DateTime Date
        {
            get => _dtpDate.Value.Date;
            set => _dtpDate.Value = value.Date;
        }
        public string Description
        {
            get => _txtDescription.Text;
            set => _txtDescription.Text = value;
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
        public Currency Currency
        {
            get => (Currency)_cbCurrency.SelectedItem;
            set => _cbCurrency.SelectedIndex = _cbCurrency.FindStringExact(value.Name);
        }

        //properties installments
        public bool IsInstallment
        {
            get => _ckbInstallments.Checked;
            set => _ckbInstallments.Checked = value;
        }
        public int InstallmentCurrent
        {
            get => IntTools.Convert(_txtInstallmentCurrent.Text);
            set => _txtInstallmentCurrent.Text = value > 0 ? value.ToString() : "";
        }
        public int InstallmentMax
        {
            get => IntTools.Convert(string.IsNullOrEmpty(_txtInstallments.Text) ? "1" : _txtInstallments.Text);
            set => _txtInstallments.Text = value > 0 ? value.ToString() : "";
        }

        //properties service
        public bool IsService
        {
            get => _ckbService.Checked;
            set => _ckbService.Checked = value;
        }
        public int Frequency
        {
            get
            {
                var fre = IntTools.Convert(_cbFrequency.SelectedItem is null ? "" : _cbFrequency.SelectedItem.ToString());
                return fre > 0 ? fre : 1;
            }
            set
            {
                if (value < 1) value = 1;
                _cbFrequency.SelectedIndex = _cbFrequency.FindString(value.ToString());
            }
        }

        public TransactionHistoryView()
        {
            using (new CursorWait())
            {
                Dock = DockStyle.Fill;
                InitializeComponent();
                ControlsSetup();
                ButtonsLogic();
            }
        }

        //methods
        public void SetCurrenciesList(List<Currency> datasource)
        {
            _cbCurrency.DataSource = datasource;
            _cbCurrency.DisplayMember = "Name";
        }

        public void GrdRefreshData(List<TransactionViewDto> datasource)
        {
            using (new CursorWait())
            using (new DataGridViewHide(_cettogrd))
            {
                //Limpio la grilla
                _cettogrd.Rows.Clear();

                if (datasource.Count <= 0)
                    return;

                var row = 0;
                foreach (var year in datasource.OrderByDescending(x => x.Date).Select(x => x.Date.Year).Distinct())
                    for (var month = 12; month >= 1; month--)
                    {
                        List<TransactionViewDto> monthTransactions = datasource
                            .Where(x => x.Date.Year == year && x.Date.Month == month).OrderByDescending(x => x.Date.Day).ThenBy(x => x.Description).ToList();

                        if (monthTransactions.Count == 0)
                            continue;

                        //Determino los separadores
                        DateTime separatorDate = new DateTime(year, month, 1);
                        GrdInsertMonthSeparator(ref row, separatorDate);

                        //Obtengo los detalles pasivos
                        var passive = monthTransactions.Where(x => x.Amount < 0)
                            .OrderByDescending(x => x.TransactionType).ToList();

                        //Obtengo los detalles activos
                        var assets = monthTransactions.Where(x => x.Amount >= 0)
                            .OrderByDescending(x => x.TransactionType).ToList();

                        //Añado los detalles services pasivos
                        if (passive.Count > 0)
                        {
                            GrdInsertAmountSeparator(ref row, true);
                            foreach (var dto in passive)
                                GrdAddRow(ref row, dto);
                        }

                        //Añado los detalles services activos
                        if (assets.Count > 0)
                        {
                            GrdInsertAmountSeparator(ref row, false);
                            foreach (var dto in assets)
                                GrdAddRow(ref row, dto);
                        }
                    }
            }
        }

        public void GrdAddInsertedRow(TransactionViewDto dto)
        {
            DateTime date = dto.Date;
            //var IsPassive = dto.Amount < 0;

            int initGroupIndex = _cettogrd.Rows.Count;
            int endGroupIndex = _cettogrd.Rows.Count;

            //Obtengo la posicion del separador con el año y mes iguales a mi dto
            for (int index = 0; index < _cettogrd.Rows.Count; index++)
            {
                //Si no es un separador, ignoro la fila
                if ((int)_cettogrd.Rows[index].Cells["id"].Value != -1)
                    continue;

                var year = IntTools.Convert(_cettogrd.Rows[index].Cells["date"].Value.ToString());
                var month = IntTools.Convert(_cettogrd.Rows[index].Cells["entity"].Value.ToString());

                //Obtengo el separador actual
                if (date.Year == year && date.Month == month)
                    initGroupIndex = index;

                //Obtengo el separador siguiente (osea el que deberia seguir a este dto)
                if (date.Year < year || (date.Year == year && date.Month < month))
                {
                    endGroupIndex = index - 1;
                    break;
                }
            }

            //Si el index del grupo es igual a la ultima fila significa que no existe, por ende lo creo desde 0
            if (initGroupIndex == _cettogrd.Rows.Count)
            {
                //Añado el separador
                GrdInsertMonthSeparator(ref initGroupIndex, date);
                //Como el separador es el ultimo en la lista actualizo el endgroupindex
                endGroupIndex = _cettogrd.Rows.Count;
            }

            //Determino si el dto es pasivo o activo
            var isPasive = dto.Amount < 0;

            //Compruebo que existan separadores por valor
            if (initGroupIndex != endGroupIndex)
            {
                //Obtengo las row index de los separadores de valor
                var passiveIndex = -1;
                var assetsIndex = -1;
                for (int index = initGroupIndex + 1; index <= endGroupIndex; index++)
                {
                    if ((int)_cettogrd.Rows[index].Cells["id"].Value != -2)
                        continue;

                    if (_cettogrd.Rows[index].Cells["entity"].Value.ToString() == "Pasivos")
                        passiveIndex = index;

                    if (_cettogrd.Rows[index].Cells["entity"].Value.ToString() == "Activos")
                        assetsIndex = index;
                }

                //Determino si es necesario insertar un separador y termino de definir los rangos del grupo de celdas
                if (isPasive)
                {
                    //Si no existe el separador lo inserto
                    if (passiveIndex == -1)
                        GrdInsertAmountSeparator(ref initGroupIndex, isPasive);
                    else
                        initGroupIndex = passiveIndex;

                    //Guardo la ultima row del grupo
                    endGroupIndex = assetsIndex != -1 ? assetsIndex - 1 : endGroupIndex;
                }
                else
                {
                    if (assetsIndex == -1)
                        GrdInsertAmountSeparator(ref initGroupIndex, isPasive);
                    else
                        initGroupIndex = assetsIndex;
                }
            }
            //Si no habia separador por mes, entonces añado el separador por valor directamente
            else
            {
                GrdInsertAmountSeparator(ref initGroupIndex, isPasive);
            }

            //Determinar en que posicion añadir al dto, de modo que quede filtrado por typo y fecha
            var insertIndex = -1;
            for (int index = initGroupIndex ; index <= endGroupIndex; index++)
            {
                insertIndex = index;
                DateTime rowDate = DateTimeTools.Convert((string)_cettogrd.Rows[index + 1].Cells["date"].Value, "yyyy-MM-dd");
                TransactionType type = (TransactionType)_cettogrd.Rows[index + 1].Cells["type"].Value;
                string description = (string)_cettogrd.Rows[index + 1].Cells["description"].Value;

                //Comparo que la transaccion sea menor o igual
                //Comparo que la fecha sea menor o igual
                //Comparo el orden alfabetico de la descripcion
                if (dto.TransactionType <= type && 
                    rowDate <= dto.Date && 
                    String.Compare(dto.Description, description) >= 0)
                    break;
            }

            GrdAddRow(ref insertIndex, dto, true);
        }

        private void GrdInsertMonthSeparator(ref int row, DateTime date, bool insert = false)
        {
            var isCollapsed = true;

            if (date.Year == DateTime.Now.Year && date.Month == DateTime.Now.Month)
                isCollapsed = false;

            //Inserto el mes
            if (insert)
                row = _cettogrd.RowsInsert(row, new object[]
                {
                    -1,
                    0,
                    0,
                    date.ToString("yyyy"),
                    date.ToString("(MM) MMM"),
                    "",
                    "",
                    "",
                    "",
                    false,
                    false,
                }, true, 0, isCollapsed);
            else
                row = _cettogrd.RowsAdd(new object[]
                {
                    -1,
                    0,
                    0,
                    date.ToString("yyyy"),
                    date.ToString("(MM) MMM"),
                    "",
                    "",
                    "",
                    "",
                    false,
                    false,
                }, true, 0, isCollapsed);

            //Separador futuro
            Color sepFutureBackColor = Color.FromArgb(252, 229, 205);
            Color sepFutureForeColor = Color.Black;
            //Separador año actual
            Color sepCurrentBackColor = Color.FromArgb(255, 153, 0);
            Color sepCurrentForeColor = Color.White;
            //Separador mes actual
            Color sepCurrentMonthBackColor = Color.FromArgb(178, 107, 0);
            //Separador antiguo
            Color sepOldestBackColor = Color.FromArgb(217, 217, 217);
            Color sepOldestForeColor = Color.Black;

            //Pinto el separador
            if (date.Year > DateTime.Now.Year)
                CettoDataGridViewTools.PaintSeparator(_cettogrd, row, sepFutureBackColor, sepFutureForeColor);

            else if (date.Year == DateTime.Now.Year && date.Month == DateTime.Now.Month)
                CettoDataGridViewTools.PaintSeparator(_cettogrd, row, sepCurrentMonthBackColor, sepCurrentForeColor);

            else if (date.Year == DateTime.Now.Year)
                CettoDataGridViewTools.PaintSeparator(_cettogrd, row, sepCurrentBackColor, sepCurrentForeColor);

            else if (date.Year < DateTime.Now.Year)
                CettoDataGridViewTools.PaintSeparator(_cettogrd, row, sepOldestBackColor, sepOldestForeColor);
        }

        private void GrdInsertAmountSeparator(ref int row, bool isPasive, bool insert = false)
        {
            var text = isPasive ? "Pasivos" : "Activos";

            if (insert)
                row = _cettogrd.RowsInsert(row, new object[]
                {
                    -2,
                    0,
                    0,
                    "",
                    text,
                    "",
                    "",
                    "",
                    "",
                    false,
                    false,
                }, false, 1, false);
            else
                row = _cettogrd.RowsAdd(new object[]
                {
                    -2,
                    0,
                    0,
                    "",
                    text,
                    "",
                    "",
                    "",
                    "",
                    false,
                    false,
                }, false, 1, false);

            //Separador Pasivos
            Color sepPassivesBackColor = Color.FromArgb(244, 204, 204);
            Color sepPassivesForeColor = Color.Black;
            //Separador Activos
            Color sepAssetsBackColor = Color.FromArgb(230, 255, 113);
            Color sepAssetsForeColor = Color.Black;

            if (isPasive)
                CettoDataGridViewTools.PaintSeparator(_cettogrd, row, sepPassivesBackColor, sepPassivesForeColor);
            else
                CettoDataGridViewTools.PaintSeparator(_cettogrd, row, sepAssetsBackColor, sepAssetsForeColor);
        }

        private void GrdAddRow(ref int row, TransactionViewDto dto, bool insert = false)
        {
            if (insert)
                row = _cettogrd.RowsInsert(row, new object[]
                {
                    dto.Id,
                    dto.TransactionType,
                    dto.Frequency,
                    dto.Date.ToString("yyyy-MM-dd"),
                    dto.EntityName,
                    dto.Description,
                    dto.Installment,
                    dto.CurrencyName,
                    dto.Amount.ToString("#,##0.00 $", CultureInfo.GetCultureInfo("es-ES")),
                    dto.Concider,
                    dto.Paid,
                }, false, 1, false);
            else
                row = _cettogrd.RowsAdd(new object[]
                {
                    dto.Id,
                    dto.TransactionType,
                    dto.Frequency,
                    dto.Date.ToString("yyyy-MM-dd"),
                    dto.EntityName,
                    dto.Description,
                    dto.Installment,
                    dto.CurrencyName,
                    dto.Amount.ToString("#,##0.00 $", CultureInfo.GetCultureInfo("es-ES")),
                    dto.Concider,
                    dto.Paid,
                }, false, 1, false);

            //Pinto el monto segun corresponda
            DataGridViewTools.PaintDecimal(_cettogrd, row, "amount");
        }





        public void GrdAddInserterValue(TransactionViewDto dto)
        {
            DateTime date = dto.Date;
            var IsPassive = dto.Amount < 0;
            //Obtengo la posicion del separador que corresponda
            int rowIndex = -1;
            for (int index = 0; index < _cettogrd.Rows.Count; index++)
            {
                if ((int)_cettogrd.Rows[index].Cells["id"].Value != 0)
                    continue;

                var year = IntTools.Convert(_cettogrd.Rows[index].Cells["date"].Value.ToString());
                var month = IntTools.Convert(_cettogrd.Rows[index].Cells["entity"].Value.ToString());
                //Comparo si ya existe un separador para la fecha de mi detalle
                if (date.Year == year && date.Month == month)
                    rowIndex = index;
            }
            //Consulto si el separador existe
            if (rowIndex != -1)
            {
                //Obtengo la ultima fila del grupo
                var lastGroudIndex = -1;
                for (int index = rowIndex + 1; index < _cettogrd.Rows.Count; index++)
                {
                    if ((int)_cettogrd.Rows[index].Cells["id"].Value == -1)
                        break;
                    lastGroudIndex = index;
                }
                //Si el separador existe, busco el separador de "Activos/Pasivos"
                int secondSeparatorIndex = -1;
                for (int index = rowIndex + 1; index < _cettogrd.Rows.Count; index++)
                {
                    if ((int)_cettogrd.Rows[index].Cells["id"].Value == -1)
                        break;
                    var separatorText = IsPassive ? "Pasivos" : "Activos";
                    if (_cettogrd.Rows[index].Cells["entity"].Value.ToString() == separatorText)
                        secondSeparatorIndex = index;
                }
                //Si el separador "Activo/Pasivo" no existe
                if (secondSeparatorIndex == -1)
                {
                    if (IsPassive)
                        AddGrdRows(_cettogrd, ref rowIndex, new List<TransactionViewDto> { dto }, "Pasivos", true, true);
                    else
                        AddGrdRows(_cettogrd, ref rowIndex, new List<TransactionViewDto> { dto }, "Activos", false, true);
                }
                else
                {
                    //Busco el final del minigrupo de "Activo/Pasivo"
                    for (int index = secondSeparatorIndex + 1; index < _cettogrd.Rows.Count; index++)
                    {
                        //Si es un separador de fecha o de "activo/pasivo"
                        if ((int)_cettogrd.Rows[index].Cells["id"].Value <= -1)
                            break;
                        else if (lastGroudIndex < index)
                            lastGroudIndex = index;
                    }
                    //Inserto el detalle
                    AddGrdRow(_cettogrd, ref lastGroudIndex, dto, true);
                }
            }
            else
            {
                //Variable para fechas de separadores
                DateTime separatorDate;
                //Obtengo el index de la ultima fila
                rowIndex = _cettogrd.Rows.Count;
                //Obtengo la posicion del separador superior
                for (int index = 0; index < _cettogrd.Rows.Count; index++)
                {
                    if ((int)_cettogrd.Rows[index].Cells["id"].Value != -1)
                        continue;
                    var year = IntTools.Convert(_cettogrd.Rows[index].Cells["date"].Value.ToString());
                    var month = IntTools.Convert(_cettogrd.Rows[index].Cells["entity"].Value.ToString());
                    var detailDate = new DateTime(date.Year, date.Month, 1);
                    separatorDate = new DateTime(year, month, 1);
                    //Compruebo si el separador encontrado es mas antiguo que el detalle actual
                    if (detailDate > separatorDate)
                    {
                        //Le resto 1 para comenzar a añadir los registros detras de este separador
                        rowIndex = index - 1;
                    }
                }
                //Creo el separador
                separatorDate = new DateTime(date.Year, date.Month, 1);
                AddGrdMonthSeparator(_cettogrd, ref rowIndex, separatorDate, separatorDate.ToString("(MM) MMM"), true);
                PaintGrdMonthSeparator(_cettogrd, rowIndex, date.Year, date.Month);
                //Creo el separador de activos y pasivos, y creo la fila con el dto
                if (IsPassive)
                    AddGrdRows(_cettogrd, ref rowIndex, new List<TransactionViewDto> { dto }, "Pasivos", true, true);
                else
                    AddGrdRows(_cettogrd, ref rowIndex, new List<TransactionViewDto> { dto }, "Activos", false, true);
            }
            //Resalta la fila recién añadida
            _cettogrd.ClearSelection();
            _cettogrd.Rows[rowIndex].Selected = true;
        }

        public void GrdUpdateValue(TransactionViewDto dto)
        {
            foreach (DataGridViewRow row in _cettogrd.Rows)
            {
                if ((int)row.Cells["id"].Value == dto.Id)
                {
                    row.Cells["type"].Value = dto.TransactionType;
                    row.Cells["frequency"].Value = dto.Frequency;
                    row.Cells["date"].Value = dto.Date.ToString("yyyy-MM-dd");
                    row.Cells["description"].Value = dto.Description;
                    row.Cells["installments"].Value = dto.Installment;
                    row.Cells["currency"].Value = dto.CurrencyName;
                    row.Cells["amount"].Value = dto.Amount.ToString("#,##0.00 $", CultureInfo.GetCultureInfo("es-ES"));
                    row.Cells["concider"].Value = dto.Concider;
                    row.Cells["paid"].Value = dto.Paid;
                    _cettogrd.ClearSelection();
                    row.Selected = true;
                    break;
                }
            }
        }

        private void AddGrdRows(CettoDataGridView grd, ref int row, List<TransactionViewDto> dto, string separatorText, 
            bool isPasive, bool middleInsert = false)
        {
            for (int i = 0; i < dto.Count; i++)
            {
                //Añado separador de servicios
                if (i == 0)
                {
                    AddGrdValueSeparator(grd, ref row, separatorText, middleInsert);
                    PaintGrdValueSeparator(grd, row, isPasive);
                }

                AddGrdRow(grd, ref row, dto[i], middleInsert);
            }
        }

        private void AddGrdRow(CettoDataGridView grd, ref int row, TransactionViewDto dto, bool middleInsert = false)
        {
            if (middleInsert)
            {
                row = grd.RowsInsert(row, new object[]
                {
                    dto.Id,
                    dto.TransactionType,
                    dto.Frequency,
                    dto.Date.ToString("yyyy-MM-dd"),
                    dto.EntityName,
                    dto.Description,
                    dto.Installment,
                    dto.CurrencyName,
                    dto.Amount.ToString("#,##0.00 $", CultureInfo.GetCultureInfo("es-ES")),
                    dto.Concider,
                    dto.Paid,
                }, false, 1, false);
            }
            else
            {
                row = grd.RowsAdd(new object[]
                {
                    dto.Id,
                    dto.TransactionType,
                    dto.Frequency,
                    dto.Date.ToString("yyyy-MM-dd"),
                    dto.EntityName,
                    dto.Description,
                    dto.Installment,
                    dto.CurrencyName,
                    dto.Amount.ToString("#,##0.00 $", CultureInfo.GetCultureInfo("es-ES")),
                    dto.Concider,
                    dto.Paid,
                }, false, 1, false);
            }

            //Pinto el monto segun corresponda
            DataGridViewTools.PaintDecimal(grd, row, "amount");
        }

        private void AddGrdMonthSeparator(CettoDataGridView grd, ref int row, DateTime date, string entityRow, bool middleInsert = false)
        {
            var isCollapser = true;
            if (date.Year == DateTime.Now.Year && date.Month == DateTime.Now.Month)
                isCollapser = false;
            if (middleInsert)
            {
                row = grd.RowsInsert(row, new object[]
                {
                    -1,
                    0,
                    0,
                    date.ToString("yyyy"),
                    entityRow,
                    "",
                    "",
                    "",
                    "",
                    false,
                    false,
                }, true, 0, isCollapser);
            }
            else
            {
                row = grd.RowsAdd(new object[]
                {
                    -1,
                    0,
                    0,
                    date.ToString("yyyy"),
                    entityRow,
                    "",
                    "",
                    "",
                    "",
                    false,
                    false,
                }, true, 0, isCollapser);
            }
        }

        private void AddGrdValueSeparator(CettoDataGridView grd, ref int row, string text, bool middleInsert = false)
        {
            if (middleInsert)
            {
                row = grd.RowsInsert(row, new object[]
                {
                -2,
                0,
                0,
                "",
                text,
                "",
                "",
                "",
                "",
                false,
                false,
                }, false, 1, false);
            }
            else
            {
                row = grd.RowsAdd(new object[]
                {
                -2,
                0,
                0,
                "",
                text,
                "",
                "",
                "",
                "",
                false,
                false,
                }, false, 1, false);
            }
        }

        private void PaintGrdMonthSeparator(CettoDataGridView grd, int row, int year, int month)
        {
            //Separador futuro
            Color sepFutureBackColor = Color.FromArgb(252, 229, 205);
            Color sepFutureForeColor = Color.Black;
            //Separador año actual
            Color sepCurrentBackColor = Color.FromArgb(255, 153, 0);
            Color sepCurrentForeColor = Color.White;
            //Separador mes actual
            Color sepCurrentMonthBackColor = Color.FromArgb(178, 107, 0);
            //Separador antiguo
            Color sepOldestBackColor = Color.FromArgb(217, 217, 217);
            Color sepOldestForeColor = Color.Black;

            //Pinto el separador
            if (year > DateTime.Now.Year)
                CettoDataGridViewTools.PaintSeparator(grd, row, sepFutureBackColor, sepFutureForeColor);

            else if (year == DateTime.Now.Year && month == DateTime.Now.Month)
                CettoDataGridViewTools.PaintSeparator(grd, row, sepCurrentMonthBackColor, sepCurrentForeColor);

            else if (year == DateTime.Now.Year)
                CettoDataGridViewTools.PaintSeparator(grd, row, sepCurrentBackColor, sepCurrentForeColor);

            else if (year < DateTime.Now.Year)
                CettoDataGridViewTools.PaintSeparator(grd, row, sepOldestBackColor, sepOldestForeColor);
        }

        private void PaintGrdValueSeparator(CettoDataGridView grd, int row, bool isPassive)
        {
            //Separador Pasivos
            Color sepPassivesBackColor = Color.FromArgb(244, 204, 204);
            Color sepPassivesForeColor = Color.Black;
            //Separador Activos
            Color sepAssetsBackColor = Color.FromArgb(230, 255, 113);
            Color sepAssetsForeColor = Color.Black;

            if (isPassive)
                CettoDataGridViewTools.PaintSeparator(grd, row, sepPassivesBackColor, sepPassivesForeColor);
            else
                CettoDataGridViewTools.PaintSeparator(grd, row, sepAssetsBackColor, sepAssetsForeColor);
        }

        private void GrdSetup()
        {
            DataGridViewTools.DataGridViewSetup(_cettogrd);

            //Configuracion de columnas
            _cettogrd.Columns.Add(new DataGridViewColumn()
            {
                CellTemplate = new DataGridViewTextBoxCell(),
                Name = "id",
                HeaderText = "Id",
                ReadOnly = true,
                Visible = false,
            }); //0 id
            _cettogrd.Columns.Add(new DataGridViewColumn()
            {
                CellTemplate = new DataGridViewTextBoxCell(),
                Name = "type",
                HeaderText = "Tipo",
                ReadOnly = true,
                Visible = false,
            }); //1 type
            _cettogrd.Columns.Add(new DataGridViewColumn()
            {
                CellTemplate = new DataGridViewTextBoxCell(),
                Name = "frequency",
                HeaderText = "Frecuencia",
                ReadOnly = true,
                Visible = false,
            }); //2 frequency
            _cettogrd.Columns.Add(new DataGridViewColumn()
            {
                CellTemplate = new DataGridViewTextBoxCell(),
                DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "date",
                HeaderText = "Fecha",
                Width = _colWidthDate,
                ReadOnly = true,
            }); //3 date
            _cettogrd.Columns.Add(new DataGridViewColumn()
            {
                CellTemplate = new DataGridViewTextBoxCell(),
                Name = "entity",
                HeaderText = "Entidad",
                Width = _colWidthEntity,
                ReadOnly = true,
            }); //4 entity
            _cettogrd.Columns.Add(new DataGridViewColumn()
            {
                CellTemplate = new DataGridViewTextBoxCell(),
                Name = "description",
                HeaderText = "Descripcion",
                ReadOnly = true,
            }); //5 description
            _cettogrd.Columns.Add(new DataGridViewColumn()
            {
                CellTemplate = new DataGridViewTextBoxCell(),
                DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "installments",
                HeaderText = "Cuotas",
                Width = _colWidthInstall,
                ReadOnly = true,
            }); //6 installments
            _cettogrd.Columns.Add(new DataGridViewColumn()
            {
                CellTemplate = new DataGridViewTextBoxCell(),
                DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "currency",
                HeaderText = "Moneda",
                Width = _colWidthCurrency,
                ReadOnly = true,
            }); //7 currency
            _cettogrd.Columns.Add(new DataGridViewColumn()
            {
                CellTemplate = new DataGridViewTextBoxCell(),
                DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "amount",
                HeaderText = "Monto",
                Width = _colWidthAmount,
                ReadOnly = true,
            }); //8 amount
            _cettogrd.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                CellTemplate = new DataGridViewCheckBoxCell(),
                DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "concider",
                HeaderText = "Sumar",
                Width = _colCheckBox,
            }); //9 concider
            _cettogrd.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                CellTemplate = new CettoDataGridViewGreenCheckBoxCell(),
                DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "paid",
                HeaderText = "Pagado",
                Width = _colCheckBox,
            }); //10 paid
        }

        private void ControlsSetup()
        {
            _txtEntityName.MaxLength = 25;
            _dtpDate.CustomFormat = ConfigurationManager.AppSettings["DateFormat"];
            _txtDescription.MaxLength = 150;
            _txtAmount.TextAlign = HorizontalAlignment.Right;
            _txtAmount.Text = "0";

            _ckbInstallments.Checked = false;
            _txtInstallmentCurrent.Enabled = false;
            _txtInstallmentCurrent.MaxLength = 2;
            _txtInstallmentCurrent.TextAlign = HorizontalAlignment.Center;
            _txtInstallments.Enabled = false;
            _txtInstallments.MaxLength = 2;
            _txtInstallments.TextAlign = HorizontalAlignment.Center;

            _ckbService.Checked = false;
            _cbFrequency.Enabled = false;
            _cbFrequency.Items.Clear();
            _cbFrequency.Items.Add("1 Mes");
            _cbFrequency.Items.Add("3 Meses");
            _cbFrequency.Items.Add("6 Meses");
            _cbFrequency.Items.Add("12 Meses");
            _cbFrequency.SelectedIndex = _cbFrequency.FindString("1");

            GrdSetup();
        }

        private void ButtonsLogic()
        {
            var isCreditCardRest = _selectedDto?.TransactionType == TransactionType.CreditCardOutstanding;
            var isService = _selectedDto?.TransactionType == TransactionType.Service;

            _tsbInsert.Enabled = _selectedDto == null;
            _tsbUpdate.Enabled = _selectedDto != null && !isCreditCardRest;
            _tsbDelete.Enabled = _selectedDto != null;
            _tsbNewPay.Enabled = _selectedDto != null && isCreditCardRest;

            _dtpDate.Enabled = !(isCreditCardRest || isService);

            _ckbInstallments.Enabled = _selectedDto == null;
            _ckbService.Enabled = _selectedDto == null;
        }

        private void Clear()
        {
            _selectedDto = null;
            _checkBoxChangeDto = null;
            _focusRow = 0;

            ClearInputs();
        }

        private void ClearInputs()
        {
            _txtEntityName.Text = "";
            _txtDescription.Text = "";
            _txtAmount.Text = "0";
            _cbCurrency.SelectedIndex = _cbCurrency.FindStringExact("ARS");

            _ckbInstallments.Checked = false;
            _txtInstallmentCurrent.Text = "";
            _txtInstallments.Text = "";

            _ckbService.Checked = false;
            _cbFrequency.SelectedIndex = _cbFrequency.FindString("1");

            ButtonsLogic();
        }

        //events
        private void _tsbNewPay_Click(object sender, EventArgs e)
        {
            ButtonNewPayClick.Invoke(sender, e);
        }

        private void _tsbInsert_Click(object sender, EventArgs e)
        {
            ButtonInsertClick.Invoke(sender, e);
            Clear();
            ButtonsLogic();
        }

        private void _tsbUpdate_Click(object sender, EventArgs e)
        {
            ButtonUpdateClick.Invoke(sender, e);
            Clear();
        }

        private void _tsbDelete_Click(object sender, EventArgs e)
        {
            ButtonDeleteClick.Invoke(sender, e);
            Clear();
            ButtonsLogic();
        }

        private void _tsbClear_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void _tsbExit_Click(object sender, EventArgs e)
        {
            ButtonExitClick.Invoke(sender, e);
        }

        private void _btnEntitySearch_Click(object sender, EventArgs e)
        {
            ButtonEntitySearchClick.Invoke(sender, e);
        }

        private void _ckbInstallments_CheckedChanged(object sender, EventArgs e)
        {
            _txtInstallments.Enabled = _ckbInstallments.Checked;

            if (_ckbInstallments.Checked)
                _ckbService.Checked = false;
        }

        private void _ckbService_CheckedChanged(object sender, EventArgs e)
        {
            _cbFrequency.Enabled = _ckbService.Checked;

            if (_ckbService.Checked)
                _ckbInstallments.Checked = false;
        }

        private void _grd_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var grd = sender as DataGridView;

            if (e.RowIndex < 0)
                return;

            //Si es un separador
            if ((int)grd.Rows[e.RowIndex].Cells["id"].Value <= 0)
                return;

            //Si no se esta editando un detalle, actualizo la fecha para crear una transaccion
            if (_selectedDto is null)
                this.Date = DateTimeTools.Convert((string)grd.Rows[e.RowIndex].Cells["date"].Value, "yyyy-MM-dd");
        }

        private void _grd_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var grd = sender as DataGridView;

            //Si es un separador
            if ((int)grd.Rows[e.RowIndex].Cells["id"].Value < 0)
                return;

            //Si el doble click es en los checkbox
            if (e.ColumnIndex == 13 || e.ColumnIndex == 14)
                return;

            _selectedDto = new TransactionViewDto
            {
                Id = (int)grd.Rows[e.RowIndex].Cells["id"].Value,
                TransactionType = (TransactionType)grd.Rows[e.RowIndex].Cells["type"].Value,
                Frequency = (int)grd.Rows[e.RowIndex].Cells["frequency"].Value,
                Date = DateTimeTools.Convert((string)grd.Rows[e.RowIndex].Cells["date"].Value, "yyyy-MM-dd"),
                EntityName = (string)grd.Rows[e.RowIndex].Cells["entity"].Value,
                Description = (string)grd.Rows[e.RowIndex].Cells["description"].Value,
                Installment = (string)grd.Rows[e.RowIndex].Cells["installments"].Value,
                CurrencyName = (string)grd.Rows[e.RowIndex].Cells["currency"].Value,
                Amount = DecimalTools.Convert((string)grd.Rows[e.RowIndex].Cells["amount"].Value),
                Concider = (bool)grd.Rows[e.RowIndex].Cells["concider"].Value,
                Paid = (bool)grd.Rows[e.RowIndex].Cells["paid"].Value,
            };

            ClearInputs();
            GrdDoubleClick.Invoke(sender, e);
            ButtonsLogic();
        }

        private void _grd_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var grd = sender as DataGridView;

            //Si es un separador
            if ((int)grd.Rows[e.RowIndex].Cells["id"].Value < 0)
                return;

            //si el click NO es en los checkbox
            if (e.ColumnIndex != 13 && e.ColumnIndex != 14)
                return;

            _checkBoxChangeDto = new TransactionViewDto
            {
                Id = (int)grd.Rows[e.RowIndex].Cells["id"].Value,
                TransactionType = (TransactionType)grd.Rows[e.RowIndex].Cells["type"].Value,
                Frequency = (int)grd.Rows[e.RowIndex].Cells["frequency"].Value,
                Date = DateTimeTools.Convert((string)grd.Rows[e.RowIndex].Cells["date"].Value, "yyyy-MM-dd"),
                EntityName = (string)grd.Rows[e.RowIndex].Cells["entity"].Value,
                Description = (string)grd.Rows[e.RowIndex].Cells["description"].Value,
                Installment = (string)grd.Rows[e.RowIndex].Cells["installments"].Value,
                CurrencyName = (string)grd.Rows[e.RowIndex].Cells["currency"].Value,
                Amount = DecimalTools.Convert((string)grd.Rows[e.RowIndex].Cells["amount"].Value),
                Concider = (bool)grd.Rows[e.RowIndex].Cells["concider"].Value,
                Paid = (bool)grd.Rows[e.RowIndex].Cells["paid"].Value,
            };
            GrdValueChange.Invoke(sender, e);
        }

        private void _grd_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            //Si la fila es la de los headers
            if (e.RowIndex < 0)
                return;

            Color cellBorder = Color.FromArgb(50, 50, 50);

            //Consulto si la fila es un separador
            var isSeparator = (int)_cettogrd.Rows[e.RowIndex].Cells["id"].Value < 0;

            if (e.ColumnIndex == 3)
            {
                // Dibuja el contenido predeterminado de la celda
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);
                // Pinto el borde derecho de la fecha
                DataGridViewTools.PaintCellBorder(e, cellBorder, DataGridViewBorder.RightBorder);
                e.Handled = true;
                return;
            }

            if (isSeparator)
            {
                // Dibuja el contenido predeterminado de la celda
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);

                //Evito que se muestren checkboxes en los separadores
                if (e.ColumnIndex == 13 || e.ColumnIndex == 14)
                    e.PaintBackground(e.CellBounds, true);

                // Pinto el borde derecho de la fecha
                if (e.ColumnIndex == 7)
                    DataGridViewTools.PaintCellBorder(e, cellBorder, DataGridViewBorder.RightBorder);

                // Pinto el borde inferior
                DataGridViewTools.PaintCellBorder(e, cellBorder, DataGridViewBorder.BottomBorder);

                e.Handled = true;
            }
            else
            {
                // Dibuja el contenido predeterminado de la celda
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);

                // Pinto el borde derecho de la fecha
                if (e.ColumnIndex == 7)
                    DataGridViewTools.PaintCellBorder(e, cellBorder, DataGridViewBorder.RightBorder);

                // Indica que hemos manejado el evento y no se requiere el dibujo predeterminado
                e.Handled = true;
            }
        }

        private void _grd_Resize(object sender, EventArgs e)
        {
            _cettogrd.Columns["description"].Width = _cettogrd.Width - _cettogrd.ExpandColumnHeight - _colWidthTotal - 19;
        }

        public event EventHandler ButtonInsertClick;
        public event EventHandler ButtonNewPayClick;
        public event EventHandler ButtonUpdateClick;
        public event EventHandler ButtonDeleteClick;
        public event EventHandler ButtonExitClick;
        public event EventHandler ButtonEntitySearchClick;
        public event EventHandler GrdDoubleClick;
        public event EventHandler GrdValueChange;
    }

    internal class RowItem
    {
        public int RowId { get; set; }
        public int DetailId { get; set; }
        public DateTime Date { get; set; }
        public int DistanceToSeparator { get; set; }
    }
}
