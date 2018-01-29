﻿Imports System.Data
Imports Mensaje
Imports System.DateTime
Imports System.Data.SqlClient

Partial Class MesaControl_Gestion
    Inherits System.Web.UI.Page

    Private dtConsulta As DataTable
    Private dtConsultaMov As DataTable

    Private Enum Operacion
        Ninguna
        Nuevo
        Modifica
        Consulta
        Anula
    End Enum

    Private Enum Columnas
        Incendio = 0
        Hidro = 1
        Terremoto = 2
        RCAviones = 3
        Cascos = 4
        Guerra = 5
    End Enum

    Private Enum Ramo
        Incendio = 902
        Hidro = 903
        Terremoto = 904
        RCAviones = 913
        CascosAereos = 916
    End Enum

    Public Property dtTablero() As DataTable
        Get
            Return Session("dtTablero")
        End Get
        Set(ByVal value As DataTable)
            Session("dtTablero") = value
        End Set
    End Property

    Public Property dtRiesgo() As DataTable
        Get
            Return Session("dtRiesgo")
        End Get
        Set(ByVal value As DataTable)
            Session("dtRiesgo") = value
        End Set
    End Property

    Public Property dtAgrupacion() As DataTable
        Get
            Return Session("dtAgrupacion")
        End Get
        Set(ByVal value As DataTable)
            Session("dtAgrupacion") = value
        End Set
    End Property

    Public Property dtReparto() As DataTable
        Get
            Return Session("dtReparto")
        End Get
        Set(ByVal value As DataTable)
            Session("dtReparto") = value
        End Set
    End Property

    Public Property dtProgramaCapas() As DataTable
        Get
            Return Session("dtProgramaCapas")
        End Get
        Set(ByVal value As DataTable)
            Session("dtProgramaCapas") = value
        End Set
    End Property

    Public Property dtCapas() As DataTable
        Get
            Return Session("dtCapas")
        End Get
        Set(ByVal value As DataTable)
            Session("dtCapas") = value
        End Set
    End Property

    Public Property dtDistribucion() As DataTable
        Get
            Return Session("dtDistribucion")
        End Get
        Set(ByVal value As DataTable)
            Session("dtDistribucion") = value
        End Set
    End Property

    Public Property dtBroker() As DataTable
        Get
            Return Session("dtBroker")
        End Get
        Set(ByVal value As DataTable)
            Session("dtBroker") = value
        End Set
    End Property

    Public Property dtReasegurador() As DataTable
        Get
            Return Session("dtReasegurador")
        End Get
        Set(ByVal value As DataTable)
            Session("dtReasegurador") = value
        End Set
    End Property

    Public Property dtSubjetividad() As DataTable
        Get
            Return Session("dtSubjetividad")
        End Get
        Set(ByVal value As DataTable)
            Session("dtSubjetividad") = value
        End Set
    End Property

    Public Property dtIncisos() As DataTable
        Get
            Return Session("dtIncisos")
        End Get
        Set(ByVal value As DataTable)
            Session("dtIncisos") = value
        End Set
    End Property

    Public Property dtPolizasAseg() As DataTable
        Get
            Return Session("dtPolizasAseg")
        End Get
        Set(ByVal value As DataTable)
            Session("dtPolizasAseg") = value
        End Set
    End Property

    Public Property dtRiesgosPol() As DataTable
        Get
            Return Session("dtRiesgosPol")
        End Get
        Set(ByVal value As DataTable)
            Session("dtRiesgosPol") = value
        End Set
    End Property

    Public Property dtRepartoResumen() As DataTable
        Get
            Return Session("dtRepartoResumen")
        End Get
        Set(ByVal value As DataTable)
            Session("dtRepartoResumen") = value
        End Set
    End Property

    Public Property dtDistribucionResumen() As DataTable
        Get
            Return Session("dtDistribucionResumen")
        End Get
        Set(ByVal value As DataTable)
            Session("dtDistribucionResumen") = value
        End Set
    End Property

    Public Property dtComisionResumen() As DataTable
        Get
            Return Session("dtComisionResumen")
        End Get
        Set(ByVal value As DataTable)
            Session("dtComisionResumen") = value
        End Set
    End Property

    Public Property dtUbicaciones() As DataTable
        Get
            Return Session("dtUbicaciones")
        End Get
        Set(ByVal value As DataTable)
            Session("dtUbicaciones") = value
        End Set
    End Property

    Public Property EdoOperacion() As Integer
        Get
            Return hid_Operacion.Value
        End Get
        Set(ByVal value As Integer)
            hid_Operacion.Value = value
        End Set
    End Property

    Private Sub MesaControl_Gestion_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            If Not IsPostBack Then
                Session.Timeout = 180
                Session.Add("Session", 1)

                Master.Titulo = "Mesa de Control"

                Funciones.LlenaCatDDL(ddl_Moneda, "Mon",,,,,, False)

                Funciones.LlenaCatDDL(ddl_Agrupador, "Agr",,,,,, False)

                Funciones.LlenaCatGrid(gvd_RamoSbr, "Ras", "", "")

                EdoControl(Operacion.Ninguna)
                EdoOperacion = Operacion.Ninguna

                Master.Clase_Logo = "zona-logo-ancha"
                Master.Clase_Form = "zona-form-ancha"
                Master.tablero_visible = True
                Master.fecha_visible = False

                Limpia_Control()

            End If

            If Master.mPrefijo = "Bro" And Len(Master.mSeleccionados) > 0 Then
                Agrega_Intermediario()
                Master.mPrefijo = vbNullString
            ElseIf Master.mPrefijo = "Cia" And Len(Master.mSeleccionados) > 0 Then
                Agrega_Reasegurador()
                Master.mPrefijo = vbNullString
            ElseIf Master.mPrefijo = "Coa" And Len(Master.mSeleccionados) > 0 Then
                Agrega_Coasegurador()
                Master.mPrefijo = vbNullString
            End If

            If Session("Session") Is Nothing Then
                FormsAuthentication.SignOut()
                Response.Redirect("../Pages/Login.aspx")
                Mensaje.MuestraMensaje("Mesa de Control", "La sesión ha finalizado por inactividad", TipoMsg.Falla)
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Function fn_ObtieneDatos(ByRef dtDatos As DataTable, ByVal ArrayCampos() As String, Optional ByVal Condicion As String = vbNullString) As String()
        Dim id_Datos As Integer = 0
        Dim strDatos(0) As String
        Dim TipoDato As String
        Dim Campo As String = vbNullString

        Dim myRow() As Data.DataRow
        myRow = dtDatos.Select(Condicion)

        For Each row In myRow
            If Len(strDatos(id_Datos)) > 7500 Then
                id_Datos = id_Datos + 1
                ReDim Preserve strDatos(id_Datos)
                strDatos(id_Datos) = ""
            End If

            strDatos(id_Datos) = strDatos(id_Datos) & "(@strKey,"

            For Each elemento In ArrayCampos
                If InStr(elemento, ":") > 0 Then
                    Campo = Split(elemento, ":")(0)
                    TipoDato = Split(elemento, ":")(1)
                Else
                    Campo = elemento
                    TipoDato = "N"
                End If

                Select Case TipoDato
                    Case "N"
                        If IsNumeric(row(Campo)) Then
                            strDatos(id_Datos) = strDatos(id_Datos) & row(Campo) & ","
                        Else
                            strDatos(id_Datos) = strDatos(id_Datos) & "NULL" & ","
                        End If
                    Case "T"
                        strDatos(id_Datos) = strDatos(id_Datos) & "''" & row(Campo) & "'',"
                    Case "F"
                        If IsDate(row(Campo)) Then
                            strDatos(id_Datos) = strDatos(id_Datos) & "''" & CDate(row(Campo)).ToString("yyyyMMdd") & "'',"
                        Else
                            strDatos(id_Datos) = strDatos(id_Datos) & "NULL" & ","
                        End If
                    Case "B"
                        If IsNumeric(row(Campo)) Then
                            strDatos(id_Datos) = strDatos(id_Datos) & (-1 * row(Campo)) & ","
                        Else
                            strDatos(id_Datos) = strDatos(id_Datos) & "NULL" & ","
                        End If
                End Select
            Next
            strDatos(id_Datos) = Mid(strDatos(id_Datos), 1, Len(strDatos(id_Datos)) - 1)
            strDatos(id_Datos) = strDatos(id_Datos) & "),"
        Next

        fn_ObtieneDatos = strDatos
    End Function

    Private Function fn_ValidaCampoRequerido() As Boolean
        fn_ValidaCampoRequerido = False

        If Len(txt_SearchAse.Text) = 0 Then
            Mensaje.MuestraMensaje("Mesa de Control", "No se ha especificado el Asegurado", TipoMsg.Falla)
            txt_SearchAse.Focus()
            Exit Function
        ElseIf Len(txt_ClaveOfi.Text) = 0 Then
            Mensaje.MuestraMensaje("Mesa de Control", "No se ha especificado Oficina Regional", TipoMsg.Falla)
            txt_SearchOfi.Focus()
            Exit Function
        End If

        fn_ValidaCampoRequerido = True
    End Function

    Private Function fn_Consulta() As Boolean

        fn_Consulta = False

        Dim id_folio As Integer = CInt(Replace(Master.mtx_Folio.Text, "REAS-", ""))

        dtConsulta = New DataTable
        dtConsulta = fn_ConsultaNegocio(id_folio)

        If dtConsulta.Rows.Count > 0 Then
            fn_ConsultaRiesgos(id_folio)
            fn_ConsultaGrupos(id_folio)
            fn_ConsultaReparto(id_folio)
            fn_ConsultaDistribucion(id_folio)
            fn_ConsultaIntermediario(id_folio)
            fn_ConsultaReasegurador(id_folio)
            fn_ConsultaSubjetividad(id_folio)

            fn_Consulta = True
        End If

    End Function

    Private Sub LLena_Control()
        With dtConsulta
            Master.mtx_Oferta.Text = IIf(IsDBNull(.Rows(0)("num_oferta")), "", .Rows(0)("num_oferta"))

            txt_ClaveAseg.Text = IIf(IsDBNull(.Rows(0)("cod_aseg")), "", .Rows(0)("cod_aseg"))
            txt_AsegCorto.Text = IIf(IsDBNull(.Rows(0)("nom_corto_aseg")), "", .Rows(0)("nom_corto_aseg"))
            txt_SearchAse.Text = IIf(IsDBNull(.Rows(0)("nombre_asegurado")), "", .Rows(0)("nombre_asegurado"))
            txt_RFC.Text = IIf(IsDBNull(.Rows(0)("rfc")), "", .Rows(0)("rfc"))
            txt_DomicilioFiscal.Text = IIf(IsDBNull(.Rows(0)("domicilio_fiscal")), "", .Rows(0)("domicilio_fiscal"))
            txt_DomicilioRiesgo.Text = IIf(IsDBNull(.Rows(0)("domicilio_riesgo")), "", .Rows(0)("domicilio_riesgo"))
            ddl_Moneda.SelectedValue = IIf(IsDBNull(.Rows(0)("cod_moneda")), "", .Rows(0)("cod_moneda"))
            txt_VigIni.Text = IIf(IsDBNull(.Rows(0)("fec_inivig")), "", .Rows(0)("fec_inivig"))
            txt_VigFin.Text = IIf(IsDBNull(.Rows(0)("fec_finvig")), "", .Rows(0)("fec_finvig"))
            txt_FecEmision.Text = IIf(IsDBNull(.Rows(0)("fec_emi")), "", .Rows(0)("fec_emi"))
            txt_ClaveOfi.Text = IIf(IsDBNull(.Rows(0)("cod_suc_ofi")), "", .Rows(0)("cod_suc_ofi"))
            txt_SearchOfi.Text = IIf(IsDBNull(.Rows(0)("sucursal_ofi")), "", .Rows(0)("sucursal_ofi"))
            txt_ClaveSusc.Text = IIf(IsDBNull(.Rows(0)("cod_suscriptor")), "", .Rows(0)("cod_suscriptor"))
            txt_SearchSusc.Text = IIf(IsDBNull(.Rows(0)("suscriptor")), "", .Rows(0)("suscriptor"))
            txt_ClaveTag.Text = IIf(IsDBNull(.Rows(0)("cod_tipo_agente")), "", .Rows(0)("cod_tipo_agente"))
            txt_SearchTag.Text = IIf(IsDBNull(.Rows(0)("tipoagente")), "", .Rows(0)("tipoagente"))

            txt_ClaveAge.Text = IIf(IsDBNull(.Rows(0)("cod_agente")), "", .Rows(0)("cod_agente"))
            txt_SearchAge.Text = IIf(IsDBNull(.Rows(0)("agente")), "", .Rows(0)("agente"))
            txt_ClaveSuc.Text = IIf(IsDBNull(.Rows(0)("cod_suc")), "", .Rows(0)("cod_suc"))
            txt_SearchSuc.Text = IIf(IsDBNull(.Rows(0)("sucursal")), "", .Rows(0)("sucursal"))
            txt_ClaveRam.Text = IIf(IsDBNull(.Rows(0)("cod_ramo")), "", .Rows(0)("cod_ramo"))
            txt_SearchRam.Text = IIf(IsDBNull(.Rows(0)("ramo")), "", .Rows(0)("ramo"))
            txt_NroPoliza.Text = IIf(IsDBNull(.Rows(0)("nro_pol")), "", .Rows(0)("nro_pol"))
            txt_Sufijo.Text = IIf(IsDBNull(.Rows(0)("aaaa_endoso")), "", .Rows(0)("aaaa_endoso"))
            txt_Endoso.Text = IIf(IsDBNull(.Rows(0)("nro_endoso")), "", .Rows(0)("nro_endoso"))

            txt_ClaveGre.Text = IIf(IsDBNull(.Rows(0)("cod_grupo_endo")), "", .Rows(0)("cod_grupo_endo"))
            txt_SearchGre.Text = IIf(IsDBNull(.Rows(0)("grupo_endoso")), "", .Rows(0)("grupo_endoso"))
            txt_ClaveTte.Text = IIf(IsDBNull(.Rows(0)("cod_tipo_endo")), "", .Rows(0)("cod_tipo_endo"))
            txt_SearchTte.Text = IIf(IsDBNull(.Rows(0)("tipo_endoso")), "", .Rows(0)("tipo_endoso"))
            txt_ClaveGiro.Text = IIf(IsDBNull(.Rows(0)("cod_giro_negocio")), "", .Rows(0)("cod_giro_negocio"))
            txt_SearchGiro.Text = IIf(IsDBNull(.Rows(0)("giro_negocio")), "", .Rows(0)("giro_negocio"))
            txt_GiroAsegurado.Text = IIf(IsDBNull(.Rows(0)("giro_asegurado")), "", .Rows(0)("giro_asegurado"))
            txt_Observaciones.Text = IIf(IsDBNull(.Rows(0)("observaciones")), "", .Rows(0)("observaciones"))
        End With


        LlenaGridRiesgo(False)
        LlenaGridAgrupacion(False)

        hid_IndiceGrupo.Value = IIf(gvd_Agrupacion.Rows.Count > 0, 0, -1)

        If hid_IndiceGrupo.Value > -1 Then

            lbl_Intermediarios.Text = CType(gvd_Agrupacion.Rows(hid_IndiceGrupo.Value).FindControl("lnk_Descripcion"), LinkButton).Text
            lbl_Intermediarios.ToolTip = CType(gvd_Agrupacion.Rows(hid_IndiceGrupo.Value).FindControl("lnk_Descripcion"), LinkButton).Text

            lbl_Distribucion.Text = CType(gvd_Agrupacion.Rows(hid_IndiceGrupo.Value).FindControl("lnk_Descripcion"), LinkButton).Text
            lbl_Distribucion.ToolTip = CType(gvd_Agrupacion.Rows(hid_IndiceGrupo.Value).FindControl("lnk_Descripcion"), LinkButton).Text

            With CType(gvd_Agrupacion.Rows(hid_IndiceGrupo.Value).FindControl("lnk_Descripcion"), LinkButton)
                .Font.Bold = True
                .ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")
                .BorderColor = Drawing.ColorTranslator.FromHtml("#003A5D")
            End With

            CType(gvd_Agrupacion.Rows(hid_IndiceGrupo.Value).FindControl("txt_LimResp"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")
            CType(gvd_Agrupacion.Rows(hid_IndiceGrupo.Value).FindControl("txt_PrimaNeta"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")
            CType(gvd_Agrupacion.Rows(hid_IndiceGrupo.Value).FindControl("txt_PrimaTEV"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")
            CType(gvd_Agrupacion.Rows(hid_IndiceGrupo.Value).FindControl("txt_PrimaFHM"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")

            'Llena Información de Reparto
            LlenaGridReparto(False, gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"))

            'Llena Información de Distribución
            LlenaGridDistribucion(False, gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"))

            'Llena Información de Broker
            LlenaGridBroker(False, gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"))
            hid_IndiceBroker.Value = IIf(gvd_Intermediario.Rows.Count > 0, 0, -1)


            EstadoColumnasAgrupacion()
            ValidaRamosGrupo(CType(gvd_Agrupacion.Rows(hid_IndiceGrupo.Value).FindControl("txt_Ramos"), TextBox).Text)



            If hid_IndiceBroker.Value > -1 Then

                btn_AddCia.Attributes.Remove("disabled")
                With CType(gvd_Intermediario.Rows(hid_IndiceBroker.Value).FindControl("lnk_Clave"), LinkButton)
                    .Font.Bold = True
                    .ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")
                    .BorderColor = Drawing.ColorTranslator.FromHtml("#003A5D")
                End With

                CType(gvd_Intermediario.Rows(hid_IndiceBroker.Value).FindControl("txt_Descripcion"), TextBox).Font.Bold = True
                CType(gvd_Intermediario.Rows(hid_IndiceBroker.Value).FindControl("txt_Descripcion"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")
                CType(gvd_Intermediario.Rows(hid_IndiceBroker.Value).FindControl("txt_Descripcion"), TextBox).BorderColor = Drawing.ColorTranslator.FromHtml("#003A5D")

                lbl_Reaseguradores.Text = CType(gvd_Intermediario.Rows(hid_IndiceBroker.Value).FindControl("txt_Descripcion"), TextBox).Text
                lbl_Reaseguradores.ToolTip = CType(gvd_Intermediario.Rows(hid_IndiceBroker.Value).FindControl("txt_Descripcion"), TextBox).Text

                'Llena Información de Reaseguradores
                LlenaGridReasegurador(False, gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_grupo"), gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"))

                ' Si es mediante Intermediario
                If gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker") > 0 Then
                    LlenaGridSubjetividad(False, gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_grupo"),
                                          gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"), 0)
                    txt_Observaciones.Text = CType(gvd_Intermediario.Rows(hid_IndiceBroker.Value).FindControl("txt_Observaciones"), TextBox).Text

                    ObtieneDatosComision(gvd_Intermediario, hid_IndiceBroker.Value, {"Neta", "INC", "FHM", "TEV", "RC", "CSC", "GRA"})

                    lbl_Comisiones.Text = Mid(CType(gvd_Intermediario.Rows(hid_IndiceBroker.Value).FindControl("txt_Descripcion"), TextBox).Text, 1, 20) & "..."
                    lbl_Comisiones.ToolTip = CType(gvd_Intermediario.Rows(hid_IndiceBroker.Value).FindControl("txt_Descripcion"), TextBox).Text

                    lbl_Subjetividades.Text = Mid(CType(gvd_Intermediario.Rows(hid_IndiceBroker.Value).FindControl("txt_Descripcion"), TextBox).Text, 1, 50) & "..."
                    lbl_Subjetividades.ToolTip = CType(gvd_Intermediario.Rows(hid_IndiceBroker.Value).FindControl("txt_Descripcion"), TextBox).Text

                Else 'Si es Negocio Directo
                    hid_IndiceReas.Value = IIf(gvd_Reasegurador.Rows.Count > 0, 0, -1)
                    If hid_IndiceReas.Value > -1 Then

                        With CType(gvd_Reasegurador.Rows(hid_IndiceReas.Value).FindControl("lnk_Clave"), LinkButton)
                            .Font.Bold = True
                            .ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")
                            .BorderColor = Drawing.ColorTranslator.FromHtml("#003A5D")
                        End With

                        CType(gvd_Reasegurador.Rows(hid_IndiceReas.Value).FindControl("txt_Descripcion"), TextBox).Font.Bold = True
                        CType(gvd_Reasegurador.Rows(hid_IndiceReas.Value).FindControl("txt_Descripcion"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")
                        CType(gvd_Reasegurador.Rows(hid_IndiceReas.Value).FindControl("txt_Descripcion"), TextBox).BorderColor = Drawing.ColorTranslator.FromHtml("#003A5D")

                        If gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker") = 0 Then
                            LlenaGridSubjetividad(False,
                                      gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_grupo"),
                                      gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"),
                                      gvd_Reasegurador.DataKeys(hid_IndiceReas.Value)("cod_cia"))

                            txt_Observaciones.Text = CType(gvd_Reasegurador.Rows(hid_IndiceReas.Value).FindControl("txt_Observaciones"), TextBox).Text

                            ObtieneDatosComision(gvd_Reasegurador, hid_IndiceReas.Value, {"Neta", "INC", "FHM", "TEV", "RC", "CSC", "GRA"})

                            lbl_Comisiones.Text = Mid(CType(gvd_Reasegurador.Rows(hid_IndiceReas.Value).FindControl("txt_Descripcion"), TextBox).Text, 1, 20) & "..."
                            lbl_Comisiones.ToolTip = CType(gvd_Reasegurador.Rows(hid_IndiceReas.Value).FindControl("txt_Descripcion"), TextBox).Text

                            lbl_Subjetividades.Text = Mid(CType(gvd_Reasegurador.Rows(hid_IndiceReas.Value).FindControl("txt_Descripcion"), TextBox).Text, 1, 50) & "..."
                            lbl_Subjetividades.ToolTip = CType(gvd_Reasegurador.Rows(hid_IndiceReas.Value).FindControl("txt_Descripcion"), TextBox).Text
                        End If
                    End If
                End If
            End If
        End If

    End Sub

    Private Function fn_Inserta() As Boolean
        Dim sCnn As String = ""
        Dim sSel As String = ""
        Dim id_folio As Integer
        Dim Fec_Ini As String = "NULL"
        Dim FecFin As String = "NULL"
        Dim FecEmi As String = "NULL"
        fn_Inserta = False

        fn_ActualizaDataRiesgo()
        fn_ActualizaDataGrupo()
        SeleccionaAgrupacion(hid_IndiceGrupo.Value, hid_IndiceGrupo.Value, False)


        Dim Comando As SqlClient.SqlCommand

        sCnn = ConfigurationManager.ConnectionStrings("CadenaConexion").ConnectionString
        Dim conn As SqlConnection = New SqlConnection(sCnn)

        conn.Open()
        Dim trOP As SqlTransaction
        trOP = conn.BeginTransaction()
        Try

            If IsDate(txt_VigIni.Text) Then
                Fec_Ini = "'" & CDate(txt_VigIni.Text).ToString("yyyyMMdd") & "'"
            End If
            If IsDate(txt_VigFin.Text) Then
                FecFin = "'" & CDate(txt_VigFin.Text).ToString("yyyyMMdd") & "'"
            End If
            If IsDate(txt_FecEmision.Text) Then
                FecEmi = "'" & CDate(txt_FecEmision.Text).ToString("yyyyMMdd") & "'"
            End If

            sSel = "DECLARE @intResultado int " &
                   "EXEC spI_MesaControl  " & IIf(Len(txt_ClaveAseg.Text) = 0, "NULL", txt_ClaveAseg.Text) & "," &
                                               IIf(Len(txt_AsegCorto.Text) = 0, "NULL", "'" & txt_AsegCorto.Text & "'") & "," &
                                               IIf(Len(txt_SearchAse.Text) = 0, "NULL", "'" & txt_SearchAse.Text & "'") & "," &
                                               IIf(Len(txt_RFC.Text) = 0, "NULL", "'" & txt_RFC.Text & "'") & "," &
                                               IIf(Len(txt_DomicilioFiscal.Text) = 0, "NULL", "'" & txt_DomicilioFiscal.Text & "'") & "," &
                                               IIf(Len(txt_DomicilioRiesgo.Text) = 0, "NULL", "'" & txt_DomicilioRiesgo.Text & "'") & "," &
                                               ddl_Moneda.SelectedValue & "," &
                                               Fec_Ini & "," &
                                               FecFin & "," &
                                               FecEmi & "," &
                                               IIf(Len(txt_ClaveOfi.Text) = 0, "NULL", txt_ClaveOfi.Text) & "," &
                                               IIf(Len(txt_ClaveSusc.Text) = 0, "NULL", "'" & txt_ClaveSusc.Text & "'") & "," &
                                               IIf(Len(txt_ClaveTag.Text) = 0, "NULL", txt_ClaveTag.Text) & "," &
                                               IIf(Len(txt_ClaveAge.Text) = 0, "NULL", txt_ClaveAge.Text) & "," &
                                               IIf(Len(txt_ClaveSuc.Text) = 0, "NULL", txt_ClaveSuc.Text) & "," &
                                               IIf(Len(txt_ClaveRam.Text) = 0, "NULL", txt_ClaveRam.Text) & "," &
                                               IIf(Len(txt_NroPoliza.Text) = 0, "NULL", txt_NroPoliza.Text) & "," &
                                               IIf(Len(txt_Sufijo.Text) = 0, "NULL", txt_Sufijo.Text) & "," &
                                               IIf(Len(txt_Endoso.Text) = 0, "NULL", txt_Endoso.Text) & "," &
                                               IIf(Len(txt_ClaveGre.Text) = 0, "NULL", txt_ClaveGre.Text) & "," &
                                               IIf(Len(txt_ClaveTte.Text) = 0, "NULL", txt_ClaveTte.Text) & "," &
                                               IIf(Len(txt_ClaveGiro.Text) = 0, "NULL", txt_ClaveGiro.Text) & "," &
                                               IIf(Len(txt_GiroAsegurado.Text) = 0, "NULL", "'" & txt_GiroAsegurado.Text & "'") & "," &
                                               IIf(Len(txt_Notas.Text) = 0, "NULL", "'" & txt_Notas.Text & "'") & "," &
                                               "'" & Master.cod_usuario & "'," &
                                               "0," & ddl_Avance.SelectedValue


            Comando = New SqlClient.SqlCommand(sSel, conn)
            Comando.Transaction = trOP
            id_folio = Convert.ToInt32(Comando.ExecuteScalar())

            If id_folio > 0 Then
                If fn_InsertaMovimientos(id_folio, conn, trOP) Then
                    Mensaje.MuestraMensaje("Mesa de Control", "El Folio se registro correctamente : REAS-" & id_folio, TipoMsg.Confirma)

                    trOP.Commit()
                    conn.Close()
                    fn_Inserta = True
                End If
            Else
                Mensaje.MuestraMensaje("Mesa de Control", "El Folio no fue registrado, error en la información general", TipoMsg.Falla)
                trOP.Rollback()
                conn.Close()
            End If

        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
            trOP.Rollback()
            conn.Close()
        End Try

    End Function

    Private Function fn_Actualiza() As Boolean
        Dim sCnn As String = ""
        Dim sSel As String = ""
        Dim Fec_Ini As String = "NULL"
        Dim FecFin As String = "NULL"
        Dim FecEmi As String = "NULL"
        Dim id_folio As Integer

        fn_Actualiza = False

        fn_ActualizaDataRiesgo()
        fn_ActualizaDataGrupo()
        SeleccionaAgrupacion(hid_IndiceGrupo.Value, hid_IndiceGrupo.Value, False)

        Dim Comando As SqlClient.SqlCommand

        sCnn = ConfigurationManager.ConnectionStrings("CadenaConexion").ConnectionString
        Dim conn As SqlConnection = New SqlConnection(sCnn)

        conn.Open()
        Dim trOP As SqlTransaction
        trOP = conn.BeginTransaction()

        Try
            If IsDate(txt_VigIni.Text) Then
                Fec_Ini = "'" & CDate(txt_VigIni.Text).ToString("yyyyMMdd") & "'"
            End If
            If IsDate(txt_VigFin.Text) Then
                FecFin = "'" & CDate(txt_VigFin.Text).ToString("yyyyMMdd") & "'"
            End If
            If IsDate(txt_FecEmision.Text) Then
                FecEmi = "'" & CDate(txt_FecEmision.Text).ToString("yyyyMMdd") & "'"
            End If

            id_folio = CInt(Replace(Master.mtx_Folio.Text, "REAS-", ""))

            sSel = "EXEC spU_MesaControl  " & id_folio & "," &
                                               IIf(Len(txt_ClaveAseg.Text) = 0, "NULL", txt_ClaveAseg.Text) & "," &
                                               IIf(Len(txt_AsegCorto.Text) = 0, "NULL", "'" & txt_AsegCorto.Text & "'") & "," &
                                               IIf(Len(txt_SearchAse.Text) = 0, "NULL", "'" & txt_SearchAse.Text & "'") & "," &
                                               IIf(Len(txt_RFC.Text) = 0, "NULL", "'" & txt_RFC.Text & "'") & "," &
                                               IIf(Len(txt_DomicilioFiscal.Text) = 0, "NULL", "'" & txt_DomicilioFiscal.Text & "'") & "," &
                                               IIf(Len(txt_DomicilioRiesgo.Text) = 0, "NULL", "'" & txt_DomicilioRiesgo.Text & "'") & "," &
                                               ddl_Moneda.SelectedValue & "," &
                                               Fec_Ini & "," &
                                               FecFin & "," &
                                               FecEmi & "," &
                                               IIf(Len(txt_ClaveOfi.Text) = 0, "NULL", txt_ClaveOfi.Text) & "," &
                                               IIf(Len(txt_ClaveSusc.Text) = 0, "NULL", "'" & txt_ClaveSusc.Text & "'") & "," &
                                               IIf(Len(txt_ClaveTag.Text) = 0, "NULL", txt_ClaveTag.Text) & "," &
                                               IIf(Len(txt_ClaveAge.Text) = 0, "NULL", txt_ClaveAge.Text) & "," &
                                               IIf(Len(txt_ClaveSuc.Text) = 0, "NULL", txt_ClaveSuc.Text) & "," &
                                               IIf(Len(txt_ClaveRam.Text) = 0, "NULL", txt_ClaveRam.Text) & "," &
                                               IIf(Len(txt_NroPoliza.Text) = 0, "NULL", txt_NroPoliza.Text) & "," &
                                               IIf(Len(txt_Sufijo.Text) = 0, "NULL", txt_Sufijo.Text) & "," &
                                               IIf(Len(txt_Endoso.Text) = 0, "NULL", txt_Endoso.Text) & "," &
                                               IIf(Len(txt_ClaveGre.Text) = 0, "NULL", txt_ClaveGre.Text) & "," &
                                               IIf(Len(txt_ClaveTte.Text) = 0, "NULL", txt_ClaveTte.Text) & "," &
                                               IIf(Len(txt_ClaveGiro.Text) = 0, "NULL", txt_ClaveGiro.Text) & "," &
                                               IIf(Len(txt_GiroAsegurado.Text) = 0, "NULL", "'" & txt_GiroAsegurado.Text & "'") & "," &
                                               IIf(Len(txt_Notas.Text) = 0, "NULL", "'" & txt_Notas.Text & "'") & "," &
                                               "'" & Master.cod_usuario & "'," &
                                                "0," & ddl_Avance.SelectedValue

            Comando = New SqlClient.SqlCommand(sSel, conn)
            Comando.Transaction = trOP

            If Convert.ToInt32(Comando.ExecuteScalar()) = 1 Then
                If fn_InsertaMovimientos(id_folio, conn, trOP) Then
                    Mensaje.MuestraMensaje("Mesa de Control", "Se ha actualizado correctamente el Folio : REAS-" & id_folio, TipoMsg.Confirma)
                    trOP.Commit()
                    conn.Close()

                    fn_Actualiza = True
                End If
            Else
                Mensaje.MuestraMensaje("Mesa de Control", "El Folio " & id_folio & " no fue actualizado, error en la información general", TipoMsg.Falla)
                trOP.Rollback()
                conn.Close()
            End If

        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
            trOP.Rollback()
            conn.Close()
        End Try
    End Function

    Private Function fn_InsertaMovimientos(ByVal id_folio As Integer,
                                        ByRef conexion As SqlConnection,
                                        ByRef transaccion As SqlTransaction) As Boolean

        fn_InsertaMovimientos = False

        If fn_InsertaRegistros("rRMC_RiesgosMC", id_folio, fn_ObtieneDatos(dtRiesgo, {"cod_inciso", "sn_seleccionado:B", "cod_ramo", "cod_subramo", "cod_riesgo", "cod_ind_cob", "sn_facultativo:B",
                                                                                           "sn_adicional:B", "valores_totales", "suma_asegurada", "prima_neta", "prima_inc", "prima_fhm",
                                                                                           "prima_tev", "prima_rc", "prima_casco", "prima_guerra", "prc_com_age", "com_agente",
                                                                                           "prc_com_adi_age", "com_adi_agente", "cuota", "prc_FeeGMX", "mnt_FeeGMX", "prc_ComFac", "mnt_ComFac"}, "cod_inciso > 0"), conexion, transaccion) Then

            If fn_InsertaRegistros("rGMC_GruposMC", id_folio, fn_ObtieneDatos(dtAgrupacion, {"cod_grupo", "cod_agrupador", "suma_asegurada", "prima_neta", "prima_inc", "prima_fhm",
                                                                                          "prima_tev", "prima_rc", "prima_casco", "prima_guerra", "incisos:T", "ramos:T"}), conexion, transaccion) Then

                If fn_InsertaRegistros("rRMC_RepartoMC", id_folio, fn_ObtieneDatos(dtReparto, {"cod_grupo", "cod_reparto", "sn_lider:B", "prc_part", "suma_asegurada", "prima_neta", "prima_inc", "prima_fhm",
                                                                                            "prima_tev", "prima_rc", "prima_casco", "prima_guerra"}, "cod_grupo > 0 AND cod_reparto > -1"), conexion, transaccion) Then

                    If fn_InsertaRegistros("rDMC_DistribucionMC", id_folio, fn_ObtieneDatos(dtDistribucion, {"cod_grupo", "cod_distribucion", "prc_part", "prc_partGMX", "suma_asegurada", "prima_neta", "prima_inc", "prima_fhm",
                                                                                                          "prima_tev", "prima_rc", "prima_casco", "prima_guerra"}, "cod_grupo > 0"), conexion, transaccion) Then

                        If fn_InsertaRegistros("rBMC_BrokersMC", id_folio, fn_ObtieneDatos(dtBroker, {"cod_grupo", "cod_broker", "prc_partCoas", "prc_part", "suma_asegurada", "prima_neta", "prc_com", "comision", "pnr",
                                                                                                   "prima_inc", "prc_com_inc", "com_inc", "pnr_inc", "prima_fhm", "prc_com_fhm", "com_fhm", "pnr_fhm",
                                                                                                   "prima_tev", "prc_com_tev", "com_tev", "pnr_tev", "prima_rc", "prc_com_rc", "com_rc", "pnr_rc", "prima_casco",
                                                                                                   "prc_com_casco", "com_casco", "pnr_casco", "prima_guerra", "prc_com_guerra", "com_guerra", "pnr_guerra", "prc_corretaje", "corretaje",
                                                                                                   "estatus", "observaciones:T"}), conexion, transaccion) Then

                            If fn_InsertaRegistros("rRMC_ReaseguradoresMC", id_folio, fn_ObtieneDatos(dtReasegurador, {"cod_grupo", "cod_broker", "cod_cia", "prc_part100", "prc_part", "suma_asegurada", "prima_neta", "prc_com", "comision", "pnr",
                                                                                                                    "prima_inc", "prc_com_inc", "com_inc", "pnr_inc", "prima_fhm", "prc_com_fhm", "com_fhm", "pnr_fhm",
                                                                                                                    "prima_tev", "prc_com_tev", "com_tev", "pnr_tev", "prima_rc", "prc_com_rc", "com_rc", "pnr_rc", "prima_casco",
                                                                                                                    "prc_com_casco", "com_casco", "pnr_casco", "prima_guerra", "prc_com_guerra", "com_guerra", "pnr_guerra",
                                                                                                                    "estatus", "observaciones:T"}), conexion, transaccion) Then

                                If fn_InsertaRegistros("rSMC_SubjetividadMC", id_folio, fn_ObtieneDatos(dtSubjetividad, {"cod_grupo", "cod_broker", "cod_cia", "num", "Fecha:F", "FechaReal:F", "Subjetividad:T", "sn_Subjetividad:B"}), conexion, transaccion) Then
                                    fn_InsertaMovimientos = True
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End If
    End Function

    Private Function fn_InsertaRegistros(ByVal Tabla As String, ByVal Key As String, ByVal Datos() As String,
                                      Optional ByVal conexion As SqlConnection = Nothing,
                                      Optional ByVal transaccion As SqlTransaction = Nothing) As Boolean
        fn_InsertaRegistros = True

        Dim intResultado As Integer
        Dim Comando As SqlClient.SqlCommand
        Dim conn As SqlConnection = conexion
        Try
            If Not Datos Is Nothing Then
                For pagina = 0 To UBound(Datos)
                    If Len(Datos(pagina)) > 0 Then
                        Datos(pagina) = Mid(Datos(pagina), 1, Len(Datos(pagina)) - 1)
                        Comando = New SqlClient.SqlCommand("spI_OfGread '" & Tabla & "','" & Key & "','" & Datos(pagina) & "'", conn)
                        Comando.Transaction = transaccion
                        intResultado = Convert.ToInt32(Comando.ExecuteScalar())
                    End If
                Next
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
            transaccion.Rollback()
            conn.Close()
            Return False
        End Try
    End Function

    Private Sub Limpia_Control()
        hid_IndiceNota.Value = -1
        hid_IndiceGrupo.Value = -1
        hid_IndiceGrupoDet.Value = -1
        hid_IndiceBroker.Value = -1
        hid_IndiceReas.Value = -1

        Funciones.EjecutaFuncion("fn_CerrarModalSimple('#Notas');", "CierraNotas")
        Funciones.EjecutaFuncion("fn_CerrarModalSimple('#Comisiones');", "CierraComisiones")
        Funciones.EjecutaFuncion("fn_CerrarModalSimple('#Subjetividad');", "CierraSubjetividad")

        LlenaGridTablero(True)
        LlenaGridRiesgo(True)
        LlenaGridAgrupacion(True)
        LlenaGridReparto(True, 0)
        LlenaGridProgramaCapas(True, 0)
        LlenaGridCapas(True, 0)
        LlenaGridDistribucion(True, 0)
        LlenaGridBroker(True)
        LlenaGridReasegurador(True)
        LlenaGridSubjetividad(True, 0, 0)
        LlenaGridIncisos(True)

        Master.mtx_Oferta.Text = vbNullString
        txt_NotaProceso.Text = vbNullString
        txt_SearchAse.Text = vbNullString
        txt_AsegCorto.Text = vbNullString
        txt_RFC.Text = vbNullString
        txt_DomicilioFiscal.Text = vbNullString
        txt_DomicilioRiesgo.Text = vbNullString
        ddl_Moneda.SelectedIndex = 0
        txt_VigIni.Text = vbNullString
        txt_VigFin.Text = vbNullString
        txt_FecEmision.Text = vbNullString

        txt_ClaveOfi.Text = vbNullString
        txt_ClaveSusc.Text = vbNullString
        txt_ClaveTag.Text = vbNullString
        txt_ClaveAge.Text = vbNullString

        txt_SearchOfi.Text = vbNullString
        txt_SearchSusc.Text = vbNullString
        txt_SearchTag.Text = vbNullString
        txt_SearchAge.Text = vbNullString

        txt_ClaveSuc.Text = vbNullString
        txt_SearchSuc.Text = vbNullString
        txt_ClaveRam.Text = vbNullString
        txt_SearchRam.Text = vbNullString
        txt_NroPoliza.Text = vbNullString
        txt_Sufijo.Text = vbNullString
        txt_Endoso.Text = vbNullString
        txt_SearchGre.Text = vbNullString
        txt_SearchTte.Text = vbNullString
        txt_SearchGiro.Text = vbNullString
        txt_GiroAsegurado.Text = vbNullString
        txt_Notas.Text = vbNullString

        lbl_Distribucion.Text = vbNullString
        lbl_Distribucion.ToolTip = vbNullString
        lbl_Intermediarios.Text = vbNullString
        lbl_Intermediarios.ToolTip = vbNullString
        lbl_Reaseguradores.Text = vbNullString
        lbl_Reaseguradores.ToolTip = vbNullString
        lbl_Comisiones.Text = vbNullString
        lbl_Comisiones.ToolTip = vbNullString
        lbl_Subjetividades.Text = vbNullString
        lbl_Subjetividades.ToolTip = vbNullString


        txt_PrcComNeta.Text = "0.0000"
        txt_PrcComINC.Text = "0.0000"
        txt_PrcComTEV.Text = "0.0000"
        txt_PrcComFHM.Text = "0.0000"

        txt_ComNeta.Text = "0.00"
        txt_ComINC.Text = "0.00"
        txt_ComTEV.Text = "0.00"
        txt_ComFHM.Text = "0.00"

        txt_PnrNeta.Text = "0.00"
        txt_PnrINC.Text = "0.00"
        txt_PnrTEV.Text = "0.00"
        txt_PnrFHM.Text = "0.00"

        ValidaRamosGrupo("0")
        EstadoColumnasAgrupacion()

        lbl_Asegurado.Text = vbNullString
        lbl_Poliza.Text = vbNullString
        lbl_Endoso.Text = vbNullString
        lbl_Renovacion.Text = vbNullString
        txt_VigenciaIni.Text = vbNullString
        txt_VigenciaFin.Text = vbNullString
        txt_Moneda.Text = ddl_Moneda.SelectedItem.Text
        txt_FechaEmision.Text = vbNullString
        txt_Suscriptor.Text = vbNullString
        txt_Movimiento.Text = vbNullString
        txt_FecIniColoc.Text = vbNullString
        txt_Coberturas.Text = vbNullString
        txt_FecFinColoc.Text = vbNullString

        LlenaGridRepartoResumen(True, 0)
        LlenaGridDistribucionResumen(True, 0)
        LlenaGridComisionResumen(True, 0)

        txt_ObservacionesResumen.Text = vbNullString
        txt_GarantiasPago.Text = vbNullString

        txt_ClaveResp.Text = vbNullString
        txt_SearchResp.Text = vbNullString
        txt_Departamento.Text = vbNullString

        ddl_Ramo.DataSource = Nothing
        ddl_Ramo.DataBind()

        ddl_Subramo.DataSource = Nothing
        ddl_Subramo.DataBind()
    End Sub

    Private Sub EdoControl(ByVal intOperacion As Integer)
        Select Case intOperacion
            Case Operacion.Consulta, Operacion.Anula
                Master.mtx_Folio.Enabled = False
                Master.mgv_Tablero.Enabled = False

                txt_NotaProceso.Enabled = False
                txt_SearchAse.Enabled = False
                txt_AsegCorto.Enabled = False
                txt_RFC.Enabled = False
                txt_DomicilioFiscal.Enabled = False
                txt_DomicilioRiesgo.Enabled = False
                ddl_Moneda.Enabled = False
                txt_VigIni.Enabled = False
                txt_VigFin.Enabled = False
                txt_FecEmision.Enabled = False
                btn_buscaPol.Enabled = False

                txt_ClaveOfi.Enabled = False
                txt_ClaveAge.Enabled = False

                txt_SearchOfi.Enabled = False
                txt_SearchSusc.Enabled = False
                txt_SearchTag.Enabled = False
                txt_SearchAge.Enabled = False

                txt_ClaveSuc.Enabled = False
                txt_SearchSuc.Enabled = False
                txt_ClaveRam.Enabled = False
                txt_SearchRam.Enabled = False
                txt_NroPoliza.Enabled = False
                txt_Sufijo.Enabled = False
                txt_Endoso.Enabled = False
                txt_SearchGre.Enabled = False
                txt_SearchTte.Enabled = False
                txt_SearchGiro.Enabled = False
                txt_GiroAsegurado.Enabled = False
                txt_Notas.Enabled = False
                btn_InfoRiesgos.Enabled = False

                btn_AgrupaRiesgo.Attributes.Add("disabled", "true")
                btn_AddRiesgo.Attributes.Add("disabled", "true")
                btn_RemoveRiesgo.Attributes.Add("disabled", "true")

                gvd_Riesgo.Enabled = False
                gvd_Agrupacion.Enabled = False
                gvd_Reparto.Enabled = False
                gvd_Capas.Enabled = False
                gvd_Distribucion.Enabled = False
                gvd_Intermediario.Enabled = False
                gvd_Reasegurador.Enabled = False

                '--------------------------------------
                btn_AddReparto.Attributes.Add("disabled", "true")
                btn_AddBroker.Attributes.Add("disabled", "true")
                btn_AddCia.Attributes.Add("disabled", "true")

                btn_RemoveGrupo.Attributes.Add("disabled", "true")
                btn_RemoveReparto.Attributes.Add("disabled", "true")
                btn_RemoveBroker.Attributes.Add("disabled", "true")
                btn_RemoveCia.Attributes.Add("disabled", "true")
                '--------------------------------------

                ddl_Ramo.Enabled = False
                ddl_Subramo.Enabled = False

                LlenaGridRepartoResumen(True, 0)
                LlenaGridDistribucionResumen(True, 0)
                LlenaGridComisionResumen(True, 0)

                txt_ObservacionesResumen.Text = vbNullString
                txt_GarantiasPago.Text = vbNullString

                txt_ClaveResp.Text = vbNullString
                txt_SearchResp.Text = vbNullString
                txt_Departamento.Text = vbNullString

                ddl_Avance.Enabled = False

                chk_Proporcional.Enabled = False

                btn_Nuevo.Visible = False
                btn_Consultar.Visible = False
                btn_Modificar.Visible = IIf(intOperacion = Operacion.Anula, False, True)
                btn_Anular.Visible = IIf(intOperacion = Operacion.Anula, False, True)

                btn_Guardar.Visible = IIf(intOperacion = Operacion.Consulta, False, True)
                btn_Cancelar.Visible = True



            Case Operacion.Nuevo, Operacion.Modifica

                If EdoOperacion = Operacion.Nuevo Then
                    Master.mtx_Folio.Text = vbNullString
                End If

                Master.mtx_Folio.Enabled = False
                Master.mgv_Tablero.Enabled = True

                txt_NotaProceso.Enabled = True
                txt_SearchAse.Enabled = True
                txt_AsegCorto.Enabled = True
                txt_RFC.Enabled = True
                txt_DomicilioFiscal.Enabled = True
                txt_DomicilioRiesgo.Enabled = True
                ddl_Moneda.Enabled = True
                txt_VigIni.Enabled = True
                txt_VigFin.Enabled = True
                txt_FecEmision.Enabled = True
                btn_buscaPol.Enabled = True

                txt_ClaveOfi.Enabled = True
                txt_ClaveAge.Enabled = True

                txt_SearchOfi.Enabled = True
                txt_SearchSusc.Enabled = True
                txt_SearchTag.Enabled = True
                txt_SearchAge.Enabled = True

                txt_ClaveSuc.Enabled = True
                txt_SearchSuc.Enabled = True
                txt_ClaveRam.Enabled = True
                txt_SearchRam.Enabled = True
                txt_NroPoliza.Enabled = True
                txt_Sufijo.Enabled = True
                txt_Endoso.Enabled = True
                txt_SearchGre.Enabled = True
                txt_SearchTte.Enabled = True
                txt_SearchGiro.Enabled = True
                txt_GiroAsegurado.Enabled = True
                txt_Notas.Enabled = True
                btn_InfoRiesgos.Enabled = True

                btn_AgrupaRiesgo.Attributes.Remove("disabled")
                btn_AddRiesgo.Attributes.Remove("disabled")
                btn_RemoveRiesgo.Attributes.Remove("disabled")

                gvd_Riesgo.Enabled = True
                gvd_Agrupacion.Enabled = True
                gvd_Reparto.Enabled = True
                gvd_Capas.Enabled = True
                gvd_Distribucion.Enabled = True
                gvd_Intermediario.Enabled = True
                gvd_Reasegurador.Enabled = True
                '--------------------------------------

                btn_AddReparto.Attributes.Remove("disabled")
                btn_AddBroker.Attributes.Remove("disabled")
                btn_AddCia.Attributes.Add("disabled", "true")

                btn_RemoveGrupo.Attributes.Remove("disabled")
                btn_RemoveReparto.Attributes.Remove("disabled")
                btn_RemoveBroker.Attributes.Remove("disabled")
                btn_RemoveCia.Attributes.Remove("disabled")
                '--------------------------------------

                ddl_Ramo.Enabled = True
                ddl_Subramo.Enabled = True

                ddl_Avance.Enabled = True

                chk_Proporcional.Enabled = True

                btn_Nuevo.Visible = False
                btn_Consultar.Visible = False
                btn_Modificar.Visible = False
                btn_Anular.Visible = False

                btn_Guardar.Visible = True
                btn_Cancelar.Visible = True

            Case Operacion.Ninguna
                Master.mtx_Folio.Enabled = True
                Master.mgv_Tablero.Enabled = False

                txt_NotaProceso.Enabled = False
                txt_SearchAse.Enabled = False
                txt_AsegCorto.Enabled = False
                txt_RFC.Enabled = False
                txt_DomicilioFiscal.Enabled = False
                txt_DomicilioRiesgo.Enabled = False
                ddl_Moneda.Enabled = False
                txt_VigIni.Enabled = False
                txt_VigFin.Enabled = False
                txt_FecEmision.Enabled = False
                btn_buscaPol.Enabled = False

                txt_ClaveOfi.Enabled = False
                txt_ClaveAge.Enabled = False

                txt_SearchOfi.Enabled = False
                txt_SearchSusc.Enabled = False
                txt_SearchTag.Enabled = False
                txt_SearchAge.Enabled = False

                txt_ClaveSuc.Enabled = False
                txt_SearchSuc.Enabled = False
                txt_ClaveRam.Enabled = False
                txt_SearchRam.Enabled = False
                txt_NroPoliza.Enabled = False
                txt_Sufijo.Enabled = False
                txt_Endoso.Enabled = False
                txt_SearchGre.Enabled = False
                txt_SearchTte.Enabled = False
                txt_SearchGiro.Enabled = False
                txt_GiroAsegurado.Enabled = False
                txt_Notas.Enabled = False
                btn_InfoRiesgos.Enabled = False

                btn_AgrupaRiesgo.Attributes.Add("disabled", "true")
                btn_AddRiesgo.Attributes.Add("disabled", "true")
                btn_RemoveRiesgo.Attributes.Add("disabled", "true")

                gvd_Riesgo.Enabled = False
                gvd_Agrupacion.Enabled = False
                gvd_Reparto.Enabled = False
                gvd_Capas.Enabled = False
                gvd_Distribucion.Enabled = False
                gvd_Intermediario.Enabled = False
                gvd_Reasegurador.Enabled = False
                '--------------------------------------

                btn_AddReparto.Attributes.Add("disabled", "true")
                btn_AddCapa.Attributes.Add("disabled", "true")
                btn_AddBroker.Attributes.Add("disabled", "true")
                btn_AddCia.Attributes.Add("disabled", "true")

                btn_RemoveGrupo.Attributes.Add("disabled", "true")
                btn_RemoveReparto.Attributes.Add("disabled", "true")
                btn_RemoveCapa.Attributes.Add("disabled", "true")
                btn_RemoveBroker.Attributes.Add("disabled", "true")
                btn_RemoveCia.Attributes.Add("disabled", "true")
                '--------------------------------------

                ddl_Avance.Enabled = False

                chk_Proporcional.Enabled = False
                chk_Proporcional.Checked = False

                gvd_Capas.Attributes.Add("style", "display:none")

                ddl_Ramo.Enabled = False
                ddl_Subramo.Enabled = False

                btn_Nuevo.Visible = True
                btn_Consultar.Visible = True
                btn_Modificar.Visible = True
                btn_Anular.Visible = True

                btn_Guardar.Visible = False
                btn_Cancelar.Visible = False
        End Select
    End Sub

    Private Sub btn_Nuevo_Click(sender As Object, e As EventArgs) Handles btn_Nuevo.Click
        Try
            EdoOperacion = Operacion.Nuevo
            EdoControl(EdoOperacion)
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_Modificar_Click(sender As Object, e As EventArgs) Handles btn_Modificar.Click
        Try
            If EdoOperacion = Operacion.Consulta Then
                EdoOperacion = Operacion.Modifica
                EdoControl(EdoOperacion)
            Else
                If fn_Consulta() Then
                    LLena_Control()
                    EdoOperacion = Operacion.Modifica
                    EdoControl(EdoOperacion)
                Else
                    Mensaje.MuestraMensaje("Mesa de Control", "El Negocio con Folio: " & Master.mtx_Folio.Text & " no existe", TipoMsg.Advertencia)
                End If
            End If
        Catch ex As Exception
            EdoOperacion = Operacion.Ninguna
            EdoControl(EdoOperacion)
            Limpia_Control()
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_Consultar_Click(sender As Object, e As EventArgs) Handles btn_Consultar.Click
        Try
            If fn_Consulta() Then
                LLena_Control()
                EdoOperacion = Operacion.Consulta
                EdoControl(EdoOperacion)
            Else
                Mensaje.MuestraMensaje("Mesa de Control", "El Negocio con Folio: " & Master.mtx_Folio.Text & " no existe", TipoMsg.Advertencia)
            End If
        Catch ex As Exception
            EdoOperacion = Operacion.Ninguna
            EdoControl(EdoOperacion)
            Limpia_Control()
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_Anular_Click(sender As Object, e As EventArgs) Handles btn_Anular.Click
        Try
            If fn_Consulta() Then
                LLena_Control()
                EdoOperacion = Operacion.Anula
                EdoControl(EdoOperacion)
            Else
                Mensaje.MuestraMensaje("Mesa de Control", "El Negocio con Folio: " & Master.mtx_Folio.Text & " no existe", TipoMsg.Advertencia)
            End If
        Catch ex As Exception
            EdoOperacion = Operacion.Ninguna
            EdoControl(EdoOperacion)
            Limpia_Control()
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_Guardar_Click(sender As Object, e As EventArgs) Handles btn_Guardar.Click
        Try
            Select Case EdoOperacion
                Case Operacion.Nuevo
                    If fn_ValidaCampoRequerido() Then
                        fn_Inserta()
                    End If
                Case Operacion.Modifica
                    If fn_ValidaCampoRequerido() Then
                        fn_Actualiza()
                    End If
            End Select

            'EdoOperacion = Operacion.Ninguna
            'EdoControl(EdoOperacion)
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_Cancelar_Click(sender As Object, e As EventArgs) Handles btn_Cancelar.Click
        Try
            EdoOperacion = Operacion.Ninguna
            EdoControl(EdoOperacion)
            Limpia_Control()
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub LlenaGridTablero(ByVal blnNuevo As String)

        If blnNuevo = True Then
            dtTablero = New DataTable
            dtTablero.Columns.Add("cod_proceso")
            dtTablero.Columns.Add("fase")
            dtTablero.Columns.Add("cod_estatus")
            dtTablero.Columns.Add("proceso")
            dtTablero.Columns.Add("fecha_inicio")
            dtTablero.Columns.Add("fecha_fin")
            dtTablero.Columns.Add("lapso")
            dtTablero.Columns.Add("notas")

            dtTablero.Rows.Add(1, "SOLICITUD", "PSP", "PRODUCTOR-SUSCRIPCIÓN", "", "", 0, "")
            dtTablero.Rows.Add(2, "", "SRP", "SUSCRIPCIÓN-REASEGURO", "", "", 0, "")
            dtTablero.Rows.Add(3, "", "RCP", "REASEGURO-CONDUCTO", "", "", 0, "")
            dtTablero.Rows.Add(4, "CONFIRMA", "CRP", "CONDUCTO-REASEGURO", "", "", 0, "")
            dtTablero.Rows.Add(5, "", "RSP", "REASEGURO-SUSCRIPCIÓN", "", "", 0, "")
            dtTablero.Rows.Add(6, "", "SPP", "SUSCRIPTOR-PRODUCTOR", "", "", 0, "")
            dtTablero.Rows.Add(7, "SOLICITUD", "PSF", "PRODUCTOR-SUSCRIPCIÓN", "", "", 0, "")
            dtTablero.Rows.Add(8, "", "SRF", "SUSCRIPCIÓN-REASEGURO", "", "", 0, "")
            dtTablero.Rows.Add(9, "", "RCF", "REASEGURO-CONDUCTO", "", "", 0, "")
            dtTablero.Rows.Add(10, "CONFIRMA", "CRF", "CONDUCTO-REASEGURO", "", "", 0, "")
            dtTablero.Rows.Add(11, "", "RSF", "REASEGURO-SUSCRIPCIÓN", "", "", 0, "")
            dtTablero.Rows.Add(12, "", "SPF", "SUSCRIPTOR-PRODUCTOR", "", "", 0, "")
        End If

        Funciones.LlenaGrid(Master.mgv_Tablero, dtTablero)

        CType(Master.mgv_Tablero.Rows(0).FindControl("txt_Fase"), TextBox).BackColor = Drawing.Color.Yellow
        CType(Master.mgv_Tablero.Rows(3).FindControl("txt_Fase"), TextBox).BackColor = Drawing.Color.Yellow
        CType(Master.mgv_Tablero.Rows(6).FindControl("txt_Fase"), TextBox).BackColor = Drawing.Color.LightGreen
        CType(Master.mgv_Tablero.Rows(9).FindControl("txt_Fase"), TextBox).BackColor = Drawing.Color.LightGreen

        CType(Master.mgv_Tablero.Rows(1).FindControl("txt_Fase"), TextBox).Visible = False
        CType(Master.mgv_Tablero.Rows(2).FindControl("txt_Fase"), TextBox).Visible = False

        CType(Master.mgv_Tablero.Rows(4).FindControl("txt_Fase"), TextBox).Visible = False
        CType(Master.mgv_Tablero.Rows(5).FindControl("txt_Fase"), TextBox).Visible = False

        CType(Master.mgv_Tablero.Rows(7).FindControl("txt_Fase"), TextBox).Visible = False
        CType(Master.mgv_Tablero.Rows(8).FindControl("txt_Fase"), TextBox).Visible = False

        CType(Master.mgv_Tablero.Rows(10).FindControl("txt_Fase"), TextBox).Visible = False
        CType(Master.mgv_Tablero.Rows(11).FindControl("txt_Fase"), TextBox).Visible = False

    End Sub

    Private Sub LlenaGridRiesgo(ByVal bln_Nuevo As Boolean)

        If bln_Nuevo = True Then
            dtRiesgo = New DataTable
            dtRiesgo.Columns.Add("sn_seleccionado")
            dtRiesgo.Columns.Add("cod_inciso")
            dtRiesgo.Columns.Add("cod_ramo")
            dtRiesgo.Columns.Add("ramo_desc")
            dtRiesgo.Columns.Add("cod_subramo")
            dtRiesgo.Columns.Add("subramo_desc")
            dtRiesgo.Columns.Add("cod_riesgo")
            dtRiesgo.Columns.Add("riesgo_desc")
            dtRiesgo.Columns.Add("cod_ind_cob")
            dtRiesgo.Columns.Add("cobertura_desc")
            dtRiesgo.Columns.Add("sn_facultativo")
            dtRiesgo.Columns.Add("sn_adicional")
            dtRiesgo.Columns.Add("valores_totales", GetType(Decimal))
            dtRiesgo.Columns.Add("suma_asegurada", GetType(Decimal))
            dtRiesgo.Columns.Add("prima_neta", GetType(Decimal))
            dtRiesgo.Columns.Add("prima_inc", GetType(Decimal))
            dtRiesgo.Columns.Add("prima_fhm", GetType(Decimal))
            dtRiesgo.Columns.Add("prima_tev", GetType(Decimal))
            dtRiesgo.Columns.Add("prima_rc", GetType(Decimal))
            dtRiesgo.Columns.Add("prima_casco", GetType(Decimal))
            dtRiesgo.Columns.Add("prima_guerra", GetType(Decimal))
            dtRiesgo.Columns.Add("prc_com_age", GetType(Decimal))
            dtRiesgo.Columns.Add("com_agente", GetType(Decimal))
            dtRiesgo.Columns.Add("prc_com_adi_age", GetType(Decimal))
            dtRiesgo.Columns.Add("com_adi_agente", GetType(Decimal))
            dtRiesgo.Columns.Add("cuota", GetType(Decimal))
            dtRiesgo.Columns.Add("prc_FeeGMX", GetType(Decimal))
            dtRiesgo.Columns.Add("mnt_FeeGMX", GetType(Decimal))
            dtRiesgo.Columns.Add("prc_ComFac", GetType(Decimal))
            dtRiesgo.Columns.Add("mnt_ComFac", GetType(Decimal))
        End If

        Funciones.LlenaGrid(gvd_Riesgo, dtRiesgo)

        If gvd_Riesgo.Rows.Count > 0 Then
            Dim lbl_Inciso As Label = CType(gvd_Riesgo.Rows(0).FindControl("lbl_Inciso"), Label)

            Dim chk_Riesgo As CheckBox = CType(gvd_Riesgo.Rows(0).FindControl("chk_Riesgo"), CheckBox)

            Dim txt_SearchRamo As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_SearchRamo"), TextBox)
            Dim txt_SearchSubramo As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_SearchSubramo"), TextBox)
            Dim txt_SearchSeccion As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_SearchSeccion"), TextBox)
            Dim txt_SearchCobertura As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_SearchCobertura"), TextBox)
            Dim opt_Adicional As RadioButtonList = CType(gvd_Riesgo.Rows(0).FindControl("opt_Adicional"), RadioButtonList)
            Dim opt_Facultativo As RadioButtonList = CType(gvd_Riesgo.Rows(0).FindControl("opt_Facultativo"), RadioButtonList)

            Dim txt_ValoresTotales As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_ValoresTotales"), TextBox)
            Dim txt_LimResp As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_LimResp"), TextBox)
            Dim txt_PrimaNeta As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_PrimaNeta"), TextBox)
            Dim txt_PrimaINC As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_PrimaINC"), TextBox)
            Dim txt_PrimaTEV As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_PrimaTEV"), TextBox)
            Dim txt_PrimaFHM As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_PrimaFHM"), TextBox)
            Dim txt_PrimaRC As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_PrimaRC"), TextBox)
            Dim txt_PrimaCSC As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_PrimaCSC"), TextBox)
            Dim txt_PrimaGRA As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_PrimaGRA"), TextBox)

            Dim txt_PrcComAge As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_PrcComAge"), TextBox)
            Dim txt_ComAge As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_ComAge"), TextBox)
            Dim txt_PrcComAdiAge As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_PrcComAdiAge"), TextBox)
            Dim txt_ComAdiAge As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_ComAdiAge"), TextBox)

            Dim txt_PrcFeeGmx As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_PrcFeeGmx"), TextBox)
            Dim txt_FeeGmx As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_FeeGmx"), TextBox)
            Dim txt_PrcComFac As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_PrcComFac"), TextBox)
            Dim txt_ComFac As TextBox = CType(gvd_Riesgo.Rows(0).FindControl("txt_ComFac"), TextBox)

            chk_Riesgo.Visible = False
            lbl_Inciso.Text = ""
            txt_SearchRamo.Enabled = False
            txt_SearchSubramo.Enabled = False
            txt_SearchSeccion.Enabled = False
            txt_SearchCobertura.Enabled = False
            opt_Facultativo.Visible = False
            opt_Adicional.Visible = False

            txt_ValoresTotales.Enabled = False
            txt_LimResp.Enabled = False
            txt_PrimaNeta.Enabled = False
            txt_PrimaINC.Enabled = False
            txt_PrimaTEV.Enabled = False
            txt_PrimaFHM.Enabled = False
            txt_PrimaRC.Enabled = False
            txt_PrimaCSC.Enabled = False
            txt_PrimaGRA.Enabled = False

            txt_PrcComAge.Visible = False
            txt_ComAge.Enabled = False
            txt_PrcComAdiAge.Visible = False
            txt_ComAdiAge.Enabled = False
            txt_PrcFeeGmx.Visible = False
            txt_FeeGmx.Enabled = False
            txt_PrcComFac.Visible = False
            txt_ComFac.Enabled = False

            txt_SearchRamo.Font.Bold = True
            txt_ValoresTotales.Font.Bold = True
            txt_LimResp.Font.Bold = True
            txt_PrimaNeta.Font.Bold = True
            txt_PrimaINC.Font.Bold = True
            txt_PrimaTEV.Font.Bold = True
            txt_PrimaFHM.Font.Bold = True
            txt_PrimaRC.Font.Bold = True
            txt_PrimaCSC.Font.Bold = True
            txt_PrimaGRA.Font.Bold = True

            txt_ComAge.Font.Bold = True
            txt_ComAdiAge.Font.Bold = True
            txt_FeeGmx.Font.Bold = True
            txt_ComFac.Font.Bold = True

            txt_SearchRamo.BackColor = Drawing.Color.LightGray
            txt_SearchSubramo.BackColor = Drawing.Color.LightGray
            txt_SearchSeccion.BackColor = Drawing.Color.LightGray
            txt_SearchCobertura.BackColor = Drawing.Color.LightGray
            txt_ValoresTotales.BackColor = Drawing.Color.LightGray
            txt_LimResp.BackColor = Drawing.Color.LightGray
            txt_PrimaNeta.BackColor = Drawing.Color.LightGray
            txt_PrimaINC.BackColor = Drawing.Color.LightGray
            txt_PrimaTEV.BackColor = Drawing.Color.LightGray
            txt_PrimaFHM.BackColor = Drawing.Color.LightGray
            txt_PrimaRC.BackColor = Drawing.Color.LightGray
            txt_PrimaCSC.BackColor = Drawing.Color.LightGray
            txt_PrimaGRA.BackColor = Drawing.Color.LightGray


            txt_ComAge.BackColor = Drawing.Color.LightGray
            txt_ComAdiAge.BackColor = Drawing.Color.LightGray
            txt_FeeGmx.BackColor = Drawing.Color.LightGray
            txt_ComFac.BackColor = Drawing.Color.LightGray


            Dim Ajustes() As String = Split(hid_Ajuste.Value, "|")

            If Ajustes(0) = "0" Then
                TryCast(gvd_Riesgo.HeaderRow.FindControl("btn_SearchRamo"), ImageButton).ImageUrl = "~/Images/quitar_mini.png"
                TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchRamo"), Label).Text = Replace(TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchRamo"), Label).ID, "lbl_Search", "")
                TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchRamo"), Label).Width = 60
            Else
                TryCast(gvd_Riesgo.HeaderRow.FindControl("btn_SearchRamo"), ImageButton).ImageUrl = "~/Images/mas_mini.png"
                TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchRamo"), Label).Text = ". . ."
                TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchRamo"), Label).Width = 10
            End If

            If Ajustes(1) = "0" Then
                TryCast(gvd_Riesgo.HeaderRow.FindControl("btn_SearchSubramo"), ImageButton).ImageUrl = "~/Images/quitar_mini.png"
                TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchSubramo"), Label).Text = Replace(TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchSubramo"), Label).ID, "lbl_Search", "")
                TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchSubramo"), Label).Width = 60
            Else
                TryCast(gvd_Riesgo.HeaderRow.FindControl("btn_SearchSubramo"), ImageButton).ImageUrl = "~/Images/mas_mini.png"
                TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchSubramo"), Label).Text = ". . ."
                TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchSubramo"), Label).Width = 10
            End If

            If Ajustes(2) = "0" Then
                TryCast(gvd_Riesgo.HeaderRow.FindControl("btn_SearchSeccion"), ImageButton).ImageUrl = "~/Images/quitar_mini.png"
                TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchSeccion"), Label).Text = Replace(TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchSeccion"), Label).ID, "lbl_Search", "")
                TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchSeccion"), Label).Width = 60
            Else
                TryCast(gvd_Riesgo.HeaderRow.FindControl("btn_SearchSeccion"), ImageButton).ImageUrl = "~/Images/mas_mini.png"
                TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchSeccion"), Label).Text = ". . ."
                TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchSeccion"), Label).Width = 10
            End If

            If Ajustes(3) = "0" Then
                TryCast(gvd_Riesgo.HeaderRow.FindControl("btn_SearchCobertura"), ImageButton).ImageUrl = "~/Images/quitar_mini.png"
                TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchCobertura"), Label).Text = Replace(TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchCobertura"), Label).ID, "lbl_Search", "")
                TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchCobertura"), Label).Width = 60
            Else
                TryCast(gvd_Riesgo.HeaderRow.FindControl("btn_SearchCobertura"), ImageButton).ImageUrl = "~/Images/mas_mini.png"
                TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchCobertura"), Label).Text = ". . ."
                TryCast(gvd_Riesgo.HeaderRow.FindControl("lbl_SearchCobertura"), Label).Width = 10
            End If

            ValidaRamosRiesgo(gvd_Riesgo, 0, {11, 12, 13, 14, 15, 16})
            'Else
            '    LlenaGridRiesgo(True)
        End If
    End Sub

    Private Sub ValidaRamosRiesgo(ByRef gvd_Control As GridView, ByVal RowInicio As Integer, ByVal ArrayCols() As Integer)

        For Each col In ArrayCols
            gvd_Control.Columns(col).Visible = False
        Next

        For Each row In gvd_Control.Rows
            If row.rowIndex > RowInicio Then

                If gvd_Control.ID = "gvd_Riesgo" Then
                    TryCast(row.FindControl("chk_Riesgo"), CheckBox).Enabled = Not TryCast(row.FindControl("chk_Riesgo"), CheckBox).Checked
                    TryCast(row.FindControl("opt_Facultativo"), RadioButtonList).Enabled = Not TryCast(row.FindControl("chk_Riesgo"), CheckBox).Checked
                End If


                Select Case TryCast(row.FindControl("txt_ClaveRamo"), TextBox).Text
                    Case Ramo.Incendio
                        gvd_Control.Columns(ArrayCols(Columnas.Incendio)).Visible = True
                        TryCast(row.FindControl("txt_PrimaFHM"), TextBox).Attributes.Add("style", "display:none")
                        TryCast(row.FindControl("txt_PrimaTEV"), TextBox).Attributes.Add("style", "display:none")
                    Case Ramo.Hidro
                        gvd_Control.Columns(ArrayCols(Columnas.Hidro)).Visible = True
                        TryCast(row.FindControl("txt_PrimaINC"), TextBox).Attributes.Add("style", "display:none")
                        TryCast(row.FindControl("txt_PrimaTEV"), TextBox).Attributes.Add("style", "display:none")
                    Case Ramo.Terremoto
                        gvd_Control.Columns(ArrayCols(Columnas.Terremoto)).Visible = True
                        TryCast(row.FindControl("txt_PrimaINC"), TextBox).Attributes.Add("style", "display:none")
                        TryCast(row.FindControl("txt_PrimaFHM"), TextBox).Attributes.Add("style", "display:none")
                    Case Ramo.RCAviones
                        gvd_Control.Columns(ArrayCols(Columnas.RCAviones)).Visible = True
                        TryCast(row.FindControl("txt_PrimaCSC"), TextBox).Attributes.Add("style", "display:none")
                        TryCast(row.FindControl("txt_PrimaGRA"), TextBox).Attributes.Add("style", "display:none")
                    Case Ramo.CascosAereos
                        gvd_Control.Columns(ArrayCols(Columnas.Cascos)).Visible = True
                        gvd_Control.Columns(ArrayCols(Columnas.Guerra)).Visible = True

                        TryCast(row.FindControl("txt_PrimaRC"), TextBox).Attributes.Add("style", "display:none")
                    Case Else
                        If gvd_Control.ID = "gvd_Riesgo" Then
                            TryCast(row.FindControl("txt_PrimaNeta"), TextBox).Enabled = True
                        End If

                        TryCast(row.FindControl("txt_PrimaINC"), TextBox).Attributes.Add("style", "display:none")
                        TryCast(row.FindControl("txt_PrimaFHM"), TextBox).Attributes.Add("style", "display:none")
                        TryCast(row.FindControl("txt_PrimaTEV"), TextBox).Attributes.Add("style", "display:none")
                        TryCast(row.FindControl("txt_PrimaRC"), TextBox).Attributes.Add("style", "display:none")
                        TryCast(row.FindControl("txt_PrimaCSC"), TextBox).Attributes.Add("style", "display:none")
                        TryCast(row.FindControl("txt_PrimaGRA"), TextBox).Attributes.Add("style", "display:none")
                End Select
            End If
        Next
    End Sub


    Private Sub ValidaRamosGrupo(ByVal strRamos As String)

        Dim Ramos() As String = Split(strRamos, ",")

        gvd_Reparto.Columns(6).Visible = IIf(Ramos.Contains(Ramo.Incendio), True, False)
        gvd_Capas.Columns(7).Visible = IIf(Ramos.Contains(Ramo.Incendio), True, False)
        gvd_Distribucion.Columns(6).Visible = IIf(Ramos.Contains(Ramo.Incendio), True, False)
        gvd_Intermediario.Columns(7).Visible = IIf(Ramos.Contains(Ramo.Incendio), True, False)
        gvd_Reasegurador.Columns(10).Visible = IIf(Ramos.Contains(Ramo.Incendio), True, False)
        gvd_RiesgosGrupo.Columns(10).Visible = IIf(Ramos.Contains(Ramo.Incendio), True, False)

        gvd_Reparto.Columns(7).Visible = IIf(Ramos.Contains(Ramo.Hidro), True, False)
        gvd_Capas.Columns(8).Visible = IIf(Ramos.Contains(Ramo.Hidro), True, False)
        gvd_Distribucion.Columns(7).Visible = IIf(Ramos.Contains(Ramo.Hidro), True, False)
        gvd_Intermediario.Columns(8).Visible = IIf(Ramos.Contains(Ramo.Hidro), True, False)
        gvd_Reasegurador.Columns(11).Visible = IIf(Ramos.Contains(Ramo.Hidro), True, False)
        gvd_RiesgosGrupo.Columns(11).Visible = IIf(Ramos.Contains(Ramo.Hidro), True, False)

        gvd_Reparto.Columns(8).Visible = IIf(Ramos.Contains(Ramo.Terremoto), True, False)
        gvd_Capas.Columns(9).Visible = IIf(Ramos.Contains(Ramo.Terremoto), True, False)
        gvd_Distribucion.Columns(8).Visible = IIf(Ramos.Contains(Ramo.Terremoto), True, False)
        gvd_Intermediario.Columns(9).Visible = IIf(Ramos.Contains(Ramo.Terremoto), True, False)
        gvd_Reasegurador.Columns(12).Visible = IIf(Ramos.Contains(Ramo.Terremoto), True, False)
        gvd_RiesgosGrupo.Columns(12).Visible = IIf(Ramos.Contains(Ramo.Terremoto), True, False)

        gvd_Reparto.Columns(9).Visible = IIf(Ramos.Contains(Ramo.RCAviones), True, False)
        gvd_Capas.Columns(10).Visible = IIf(Ramos.Contains(Ramo.RCAviones), True, False)
        gvd_Distribucion.Columns(9).Visible = IIf(Ramos.Contains(Ramo.RCAviones), True, False)
        gvd_Intermediario.Columns(10).Visible = IIf(Ramos.Contains(Ramo.RCAviones), True, False)
        gvd_Reasegurador.Columns(13).Visible = IIf(Ramos.Contains(Ramo.RCAviones), True, False)
        gvd_RiesgosGrupo.Columns(13).Visible = IIf(Ramos.Contains(Ramo.RCAviones), True, False)

        gvd_Reparto.Columns(10).Visible = IIf(Ramos.Contains(Ramo.CascosAereos), True, False)
        gvd_Capas.Columns(11).Visible = IIf(Ramos.Contains(Ramo.CascosAereos), True, False)
        gvd_Distribucion.Columns(10).Visible = IIf(Ramos.Contains(Ramo.CascosAereos), True, False)
        gvd_Intermediario.Columns(11).Visible = IIf(Ramos.Contains(Ramo.CascosAereos), True, False)
        gvd_Reasegurador.Columns(14).Visible = IIf(Ramos.Contains(Ramo.CascosAereos), True, False)
        gvd_RiesgosGrupo.Columns(14).Visible = IIf(Ramos.Contains(Ramo.CascosAereos), True, False)

        gvd_Reparto.Columns(11).Visible = IIf(Ramos.Contains(Ramo.CascosAereos), True, False)
        gvd_Capas.Columns(12).Visible = IIf(Ramos.Contains(Ramo.CascosAereos), True, False)
        gvd_Distribucion.Columns(11).Visible = IIf(Ramos.Contains(Ramo.CascosAereos), True, False)
        gvd_Intermediario.Columns(12).Visible = IIf(Ramos.Contains(Ramo.CascosAereos), True, False)
        gvd_Reasegurador.Columns(15).Visible = IIf(Ramos.Contains(Ramo.CascosAereos), True, False)
        gvd_RiesgosGrupo.Columns(15).Visible = IIf(Ramos.Contains(Ramo.CascosAereos), True, False)

        div_incendio.Visible = IIf(Ramos.Contains(Ramo.Incendio), True, False)
        div_fhm.Visible = IIf(Ramos.Contains(Ramo.Hidro), True, False)
        div_tev.Visible = IIf(Ramos.Contains(Ramo.Terremoto), True, False)
        div_rc.Visible = IIf(Ramos.Contains(Ramo.RCAviones), True, False)
        div_casco.Visible = IIf(Ramos.Contains(Ramo.CascosAereos), True, False)
        div_guerra.Visible = IIf(Ramos.Contains(Ramo.CascosAereos), True, False)

        Dim bln_Total As Boolean = div_incendio.Visible Or div_fhm.Visible Or div_tev.Visible Or div_rc.Visible Or div_casco.Visible Or div_casco.Visible
        txt_PrcComNeta.Enabled = Not bln_Total
        txt_ComNeta.Enabled = Not bln_Total
        txt_PnrNeta.Enabled = Not bln_Total
    End Sub

    Private Function fn_ConsultaFolio(ByVal Folio As String) As DataTable
        fn_ConsultaFolio = New DataTable

        Dim ws As New ws_MesaControl.MesaControlClient
        fn_ConsultaFolio = Funciones.Lista_A_Datatable(ws.ObtieneIdFolio(Folio).ToList)
    End Function

    Private Function fn_ConsultaNegocio(ByVal id_folio As Integer) As DataTable
        fn_ConsultaNegocio = New DataTable

        Dim ws As New ws_MesaControl.MesaControlClient
        fn_ConsultaNegocio = Funciones.Lista_A_Datatable(ws.ObtieneNegocio(id_folio).ToList)
    End Function

    Private Function fn_ConsultaRiesgos(ByVal id_folio As Integer) As Boolean
        fn_ConsultaRiesgos = False

        Dim ws As New ws_MesaControl.MesaControlClient
        dtConsultaMov = New DataTable
        dtConsultaMov = Funciones.Lista_A_Datatable(ws.ObtieneRiesgo(id_folio).ToList)

        If dtConsultaMov.Rows.Count > 0 Then
            For Each row In dtConsultaMov.Rows
                dtRiesgo.Rows.Add(row("sn_seleccionado"), row("cod_inciso"), row("cod_ramo"), row("ramo_desc"), row("cod_subramo"), row("subramo_desc"),
                                  row("cod_riesgo"), row("riesgo_desc"), row("cod_ind_cob"), row("cobertura_desc"), row("sn_facultativo"),
                                  row("sn_adicional"), row("valores_totales"), row("suma_asegurada"), row("prima_neta"), row("prima_inc"),
                                  row("prima_fhm"), row("prima_tev"), row("prima_rc"), row("prima_casco"), row("prima_guerra"),
                                  row("prc_com_age"), row("com_agente"), row("prc_com_adi_age"), row("com_adi_agente"), row("cuota"),
                                  row("prc_FeeGMX"), row("mnt_FeeGMX"), row("prc_ComFac"), row("mnt_ComFac"))
            Next

            fn_ConsultaRiesgos = True
        End If
    End Function

    Private Function fn_ConsultaGrupos(ByVal id_folio As Integer) As Boolean
        fn_ConsultaGrupos = False

        Dim ws As New ws_MesaControl.MesaControlClient
        dtConsultaMov = New DataTable
        dtConsultaMov = Funciones.Lista_A_Datatable(ws.ObtieneGrupos(id_folio).ToList)

        If dtConsultaMov.Rows.Count > 0 Then
            fn_ConsultaGrupos = True

            For Each row In dtConsultaMov.Rows
                dtAgrupacion.Rows.Add(row("cod_grupo"), row("cod_agrupador"), row("Descripcion"), row("suma_asegurada"), row("prima_neta"),
                                      row("prima_inc"), row("prima_fhm"), row("prima_tev"), row("prima_rc"), row("prima_casco"),
                                      row("prima_guerra"), row("incisos"), row("ramos"))
            Next
        End If
    End Function

    Private Function fn_ConsultaReparto(ByVal id_folio As Integer) As Boolean
        fn_ConsultaReparto = False

        Dim ws As New ws_MesaControl.MesaControlClient
        dtConsultaMov = New DataTable
        dtConsultaMov = Funciones.Lista_A_Datatable(ws.ObtieneReparto(id_folio).ToList)

        If dtConsultaMov.Rows.Count > 0 Then
            fn_ConsultaReparto = True

            For Each row In dtConsultaMov.Rows
                dtReparto.Rows.Add(row("cod_grupo"), row("cod_reparto"), row("Descripcion"), row("prc_part"), row("sn_lider"),
                                   row("suma_asegurada"), row("prima_neta"), row("prima_inc"), row("prima_fhm"), row("prima_tev"),
                                   row("prima_rc"), row("prima_casco"), row("prima_guerra"))
            Next
        End If
    End Function

    Private Function fn_ConsultaDistribucion(ByVal id_folio As Integer) As Boolean
        fn_ConsultaDistribucion = False

        Dim ws As New ws_MesaControl.MesaControlClient
        dtConsultaMov = New DataTable
        dtConsultaMov = Funciones.Lista_A_Datatable(ws.ObtieneDistribucion(id_folio).ToList)

        If dtConsultaMov.Rows.Count > 0 Then
            fn_ConsultaDistribucion = True

            For Each row In dtConsultaMov.Rows
                dtDistribucion.Rows.Add(row("cod_grupo"), row("cod_distribucion"), row("Descripcion"), row("prc_part"), row("prc_partGMX"),
                                        row("suma_asegurada"), row("prima_neta"), row("prima_inc"), row("prima_fhm"), row("prima_tev"),
                                        row("prima_rc"), row("prima_casco"), row("prima_guerra"))
            Next
        End If
    End Function

    Private Function fn_ConsultaIntermediario(ByVal id_folio As Integer) As Boolean
        fn_ConsultaIntermediario = False

        Dim ws As New ws_MesaControl.MesaControlClient
        dtConsultaMov = New DataTable
        dtConsultaMov = Funciones.Lista_A_Datatable(ws.ObtieneBrokers(id_folio).ToList)

        If dtConsultaMov.Rows.Count > 0 Then
            fn_ConsultaIntermediario = True

            For Each row In dtConsultaMov.Rows
                dtBroker.Rows.Add(row("cod_grupo"), row("cod_broker"), row("Descripcion"), row("prc_partCoas"), row("prc_part"),
                                  row("suma_asegurada"), row("prima_neta"), row("prc_com"), row("comision"), row("pnr"),
                                  row("prima_inc"), row("prc_com_inc"), row("com_inc"), row("pnr_inc"),
                                  row("prima_fhm"), row("prc_com_fhm"), row("com_fhm"), row("pnr_fhm"),
                                  row("prima_tev"), row("prc_com_tev"), row("com_tev"), row("pnr_tev"),
                                  row("prima_rc"), row("prc_com_rc"), row("com_rc"), row("pnr_rc"),
                                  row("prima_casco"), row("prc_com_casco"), row("com_casco"), row("pnr_casco"),
                                  row("prima_guerra"), row("prc_com_guerra"), row("com_guerra"), row("pnr_guerra"),
                                  row("prc_corretaje"), row("corretaje"), row("estatus"), row("observaciones"))
            Next
        End If
    End Function

    Private Function fn_ConsultaReasegurador(ByVal id_folio As Integer) As Boolean
        fn_ConsultaReasegurador = False

        Dim ws As New ws_MesaControl.MesaControlClient
        dtConsultaMov = New DataTable
        dtConsultaMov = Funciones.Lista_A_Datatable(ws.ObtieneReaseguradores(id_folio).ToList)

        If dtConsultaMov.Rows.Count > 0 Then
            fn_ConsultaReasegurador = True

            For Each row In dtConsultaMov.Rows
                dtReasegurador.Rows.Add(row("cod_grupo"), row("cod_broker"), row("cod_cia"), row("Descripcion"),
                                        row("cod_naic"), row("calif"), row("age_Calif"), row("prc_part100"), row("prc_part"),
                                        row("suma_asegurada"), row("prima_neta"), row("prc_com"), row("comision"), row("pnr"),
                                        row("prima_inc"), row("prc_com_inc"), row("com_inc"), row("pnr_inc"),
                                        row("prima_fhm"), row("prc_com_fhm"), row("com_fhm"), row("pnr_fhm"),
                                        row("prima_tev"), row("prc_com_tev"), row("com_tev"), row("pnr_tev"),
                                        row("prima_rc"), row("prc_com_rc"), row("com_rc"), row("pnr_rc"),
                                        row("prima_casco"), row("prc_com_casco"), row("com_casco"), row("pnr_casco"),
                                        row("prima_guerra"), row("prc_com_guerra"), row("com_guerra"), row("pnr_guerra"),
                                        row("estatus"), row("observaciones"))
            Next
        End If
    End Function

    Private Function fn_ConsultaSubjetividad(ByVal id_folio As Integer) As Boolean
        fn_ConsultaSubjetividad = False

        Dim ws As New ws_MesaControl.MesaControlClient
        dtConsultaMov = New DataTable
        dtConsultaMov = Funciones.Lista_A_Datatable(ws.ObtieneSubjetividad(id_folio).ToList)

        If dtConsultaMov.Rows.Count > 0 Then
            fn_ConsultaSubjetividad = True

            For Each row In dtConsultaMov.Rows
                dtSubjetividad.Rows.Add(row("cod_grupo"), row("cod_broker"), row("cod_cia"), row("Num"), row("Fecha"), row("FechaReal"),
                                        row("Subjetividad"), row("sn_Subjetividad"))
            Next
        End If
    End Function

    Private Sub LlenaGridAgrupacion(ByVal bln_Nuevo As Boolean)

        If bln_Nuevo = True Then
            dtAgrupacion = New DataTable
            dtAgrupacion.Columns.Add("cod_grupo")
            dtAgrupacion.Columns.Add("cod_agrupador")
            dtAgrupacion.Columns.Add("Descripcion")
            dtAgrupacion.Columns.Add("suma_asegurada", GetType(Decimal))
            dtAgrupacion.Columns.Add("prima_neta", GetType(Decimal))
            dtAgrupacion.Columns.Add("prima_inc", GetType(Decimal))
            dtAgrupacion.Columns.Add("prima_fhm", GetType(Decimal))
            dtAgrupacion.Columns.Add("prima_tev", GetType(Decimal))
            dtAgrupacion.Columns.Add("prima_rc", GetType(Decimal))
            dtAgrupacion.Columns.Add("prima_casco", GetType(Decimal))
            dtAgrupacion.Columns.Add("prima_guerra", GetType(Decimal))
            dtAgrupacion.Columns.Add("incisos")
            dtAgrupacion.Columns.Add("ramos")
        End If

        Funciones.LlenaGrid(gvd_Agrupacion, dtAgrupacion)
    End Sub

    Private Sub LlenaGridReparto(ByVal bln_Nuevo As Boolean, Optional ByVal Grupo As Integer = 0)
        If bln_Nuevo = True Then
            dtReparto = New DataTable
            dtReparto.Columns.Add("cod_grupo", GetType(Integer))
            dtReparto.Columns.Add("cod_reparto", GetType(Integer))
            dtReparto.Columns.Add("Descripcion")
            dtReparto.Columns.Add("prc_part", GetType(Decimal))
            dtReparto.Columns.Add("sn_lider")
            dtReparto.Columns.Add("suma_asegurada", GetType(Decimal))
            dtReparto.Columns.Add("prima_neta", GetType(Decimal))
            dtReparto.Columns.Add("prima_inc", GetType(Decimal))
            dtReparto.Columns.Add("prima_fhm", GetType(Decimal))
            dtReparto.Columns.Add("prima_tev", GetType(Decimal))
            dtReparto.Columns.Add("prima_rc", GetType(Decimal))
            dtReparto.Columns.Add("prima_casco", GetType(Decimal))
            dtReparto.Columns.Add("prima_guerra", GetType(Decimal))
        End If

        If dtReparto.Select("cod_grupo ='" & Grupo & "'").Length = 0 Then
            dtReparto.Rows.Add(Grupo, -1, "TOTAL", 100.0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
            dtReparto.Rows.Add(Grupo, 1, "GRUPO MEXICANO DE SEGUROS, S.A. DE C.V.", 100.0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
        End If

        Funciones.LlenaGrid(gvd_Reparto, (From c In dtReparto.AsEnumerable() Where c.Field(Of Integer)("cod_grupo") = Grupo Select c).CopyToDataTable)

        For Each row In gvd_Reparto.Rows
            If row.rowIndex = 0 Then
                CType(row.FindControl("txt_Descripcion"), TextBox).Attributes("style") = "background-color: #3B6882;color: White;"

                CType(row.FindControl("chk_Sel"), CheckBox).Attributes.Add("style", "display:none")
                CType(row.FindControl("chk_Lider"), CheckBox).Attributes.Add("style", "display:none")

                CType(row.FindControl("txt_PrcPart"), TextBox).Enabled = False

                CType(row.FindControl("txt_Descripcion"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrcPart"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_LimResp"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrimaNeta"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrimaINC"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrimaFHM"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrimaTEV"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrimaRC"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrimaCSC"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrimaGRA"), TextBox).Font.Bold = True

            ElseIf row.rowIndex = 1 Then
                CType(row.FindControl("txt_Descripcion"), TextBox).Attributes("style") = "background-color: #3B6882;color: White;"
                CType(row.FindControl("chk_Sel"), CheckBox).Enabled = False
                CType(row.FindControl("chk_Lider"), CheckBox).Enabled = False
            Else
                CType(row.FindControl("txt_Descripcion"), TextBox).Attributes("style") = "background-color: lightblue;color: #3B6882;"
                CType(row.FindControl("txt_Descripcion"), TextBox).ToolTip = "Coasegurador: " & CType(row.FindControl("txt_Descripcion"), TextBox).Text
            End If
        Next
    End Sub

    Private Sub LlenaGridProgramaCapas(ByVal bln_Nuevo As Boolean, Optional ByVal Grupo As Integer = 0)
        If bln_Nuevo = True Then
            dtProgramaCapas = New DataTable
            dtProgramaCapas.Columns.Add("cod_grupo", GetType(Integer))
            dtProgramaCapas.Columns.Add("cod_capa", GetType(Integer))
            dtProgramaCapas.Columns.Add("valor_capa", GetType(Decimal))
            dtProgramaCapas.Columns.Add("exceso_capa", GetType(Decimal))
            dtProgramaCapas.Columns.Add("prc_part", GetType(Decimal))

            dtProgramaCapas.Rows.Add(0, 1, 100000000, 0, 77)
            dtProgramaCapas.Rows.Add(0, 2, 250000000, 0, 23)
            dtProgramaCapas.Rows.Add(0, 3, 150000000, 100000000, 42)
            dtProgramaCapas.Rows.Add(0, 4, 250000000, 250000000, 65)
            dtProgramaCapas.Rows.Add(0, 5, 400000000, 100000000, 35)
            dtProgramaCapas.Rows.Add(0, 6, 1500000000, 500000000, 100)
            dtProgramaCapas.Rows.Add(0, 7, 1550000000, 2000000000, 100)
        End If

        Dim qry = From c In dtProgramaCapas.AsEnumerable() Where c.Field(Of Integer)("cod_grupo") = Grupo Select c

        If qry.Count > 0 Then
            Funciones.LlenaGrid(gvd_ProgramaCapas, qry.CopyToDataTable())
        Else
            Funciones.LlenaGrid(gvd_ProgramaCapas, Nothing)
        End If
    End Sub

    Private Sub LlenaGridCapas(ByVal bln_Nuevo As Boolean, Optional ByVal Grupo As Integer = 0)
        If bln_Nuevo = True Then
            dtCapas = New DataTable
            dtCapas.Columns.Add("cod_grupo", GetType(Integer))
            dtCapas.Columns.Add("cod_capa", GetType(Integer))
            dtCapas.Columns.Add("prc_part_ret", GetType(Decimal))
            dtCapas.Columns.Add("prc_part", GetType(Decimal))
            dtCapas.Columns.Add("prc_partGMX_ret", GetType(Decimal))
            dtCapas.Columns.Add("prc_partGMX", GetType(Decimal))
            dtCapas.Columns.Add("suma_asegurada_ret", GetType(Decimal))
            dtCapas.Columns.Add("suma_asegurada", GetType(Decimal))
            dtCapas.Columns.Add("prima_neta_ret", GetType(Decimal))
            dtCapas.Columns.Add("prima_neta", GetType(Decimal))
            dtCapas.Columns.Add("prima_inc_ret", GetType(Decimal))
            dtCapas.Columns.Add("prima_inc", GetType(Decimal))
            dtCapas.Columns.Add("prima_fhm_ret", GetType(Decimal))
            dtCapas.Columns.Add("prima_fhm", GetType(Decimal))
            dtCapas.Columns.Add("prima_tev_ret", GetType(Decimal))
            dtCapas.Columns.Add("prima_tev", GetType(Decimal))
            dtCapas.Columns.Add("prima_rc_ret", GetType(Decimal))
            dtCapas.Columns.Add("prima_rc", GetType(Decimal))
            dtCapas.Columns.Add("prima_casco_ret", GetType(Decimal))
            dtCapas.Columns.Add("prima_casco", GetType(Decimal))
            dtCapas.Columns.Add("prima_guerra_ret", GetType(Decimal))
            dtCapas.Columns.Add("prima_guerra", GetType(Decimal))
        End If

        Dim qry = From c In dtCapas.AsEnumerable() Where c.Field(Of Integer)("cod_grupo") = Grupo Select c

        If qry.Count > 0 Then
            Funciones.LlenaGrid(gvd_Capas, qry.CopyToDataTable())
        Else
            Funciones.LlenaGrid(gvd_Capas, Nothing)
        End If

        Dim EstiloRetencion As String = "background-color: White;color: #3B6882;"
        Dim EstiloFacultativo As String = "background-color: lightblue;color: #3B6882;"
        For Each row In gvd_Capas.Rows
            CType(row.FindControl("txt_Capa"), TextBox).Attributes("style") = "background-color: #3B6882;color: White;"

            CType(row.FindControl("txt_Retencion"), TextBox).Attributes("style") = EstiloRetencion
            CType(row.FindControl("txt_Facultativo"), TextBox).Attributes("style") = EstiloFacultativo

            CType(row.FindControl("txt_PrcPartRET"), TextBox).Attributes("style") = EstiloRetencion
            CType(row.FindControl("txt_PrcPart"), TextBox).Attributes("style") = EstiloFacultativo

            CType(row.FindControl("txt_PrcPartGMXRET"), TextBox).Attributes("style") = EstiloRetencion
            CType(row.FindControl("txt_PrcPartGMX"), TextBox).Attributes("style") = EstiloFacultativo

            CType(row.FindControl("txt_LimRespRET"), TextBox).Attributes("style") = EstiloRetencion
            CType(row.FindControl("txt_LimResp"), TextBox).Attributes("style") = EstiloFacultativo

            CType(row.FindControl("txt_PrimaNetaRET"), TextBox).Attributes("style") = EstiloRetencion
            CType(row.FindControl("txt_PrimaNeta"), TextBox).Attributes("style") = EstiloFacultativo

            CType(row.FindControl("txt_PrimaINCRET"), TextBox).Attributes("style") = EstiloRetencion
            CType(row.FindControl("txt_PrimaINC"), TextBox).Attributes("style") = EstiloFacultativo

            CType(row.FindControl("txt_PrimaFHMRET"), TextBox).Attributes("style") = EstiloRetencion
            CType(row.FindControl("txt_PrimaFHM"), TextBox).Attributes("style") = EstiloFacultativo

            CType(row.FindControl("txt_PrimaTEVRET"), TextBox).Attributes("style") = EstiloRetencion
            CType(row.FindControl("txt_PrimaTEV"), TextBox).Attributes("style") = EstiloFacultativo

            CType(row.FindControl("txt_PrimaRCRET"), TextBox).Attributes("style") = EstiloRetencion
            CType(row.FindControl("txt_PrimaRC"), TextBox).Attributes("style") = EstiloFacultativo

            CType(row.FindControl("txt_PrimaCSCRET"), TextBox).Attributes("style") = EstiloRetencion
            CType(row.FindControl("txt_PrimaCSC"), TextBox).Attributes("style") = EstiloFacultativo

            CType(row.FindControl("txt_PrimaGRARET"), TextBox).Attributes("style") = EstiloRetencion
            CType(row.FindControl("txt_PrimaGRA"), TextBox).Attributes("style") = EstiloFacultativo
        Next
    End Sub

    Private Sub LlenaGridDistribucion(ByVal bln_Nuevo As Boolean, Optional ByVal Grupo As Integer = 0)
        If bln_Nuevo = True Then
            dtDistribucion = New DataTable
            dtDistribucion.Columns.Add("cod_grupo", GetType(Integer))
            dtDistribucion.Columns.Add("cod_distribucion", GetType(Integer))
            dtDistribucion.Columns.Add("Descripcion")
            dtDistribucion.Columns.Add("prc_part", GetType(Decimal))
            dtDistribucion.Columns.Add("prc_partGMX", GetType(Decimal))
            dtDistribucion.Columns.Add("suma_asegurada", GetType(Decimal))
            dtDistribucion.Columns.Add("prima_neta", GetType(Decimal))
            dtDistribucion.Columns.Add("prima_inc", GetType(Decimal))
            dtDistribucion.Columns.Add("prima_fhm", GetType(Decimal))
            dtDistribucion.Columns.Add("prima_tev", GetType(Decimal))
            dtDistribucion.Columns.Add("prima_rc", GetType(Decimal))
            dtDistribucion.Columns.Add("prima_casco", GetType(Decimal))
            dtDistribucion.Columns.Add("prima_guerra", GetType(Decimal))
        End If

        If dtDistribucion.Select("cod_grupo ='" & Grupo & "'").Length = 0 Then

            Dim ws As New ws_MesaControl.MesaControlClient
            dtConsulta = New DataTable
            dtConsulta = Funciones.Lista_A_Datatable(ws.ObtieneDistribucion(0).ToList)

            For Each row In dtConsulta.Rows
                dtDistribucion.Rows.Add(Grupo, row("cod_distribucion"), row("Descripcion"), row("prc_part"), row("prc_partGMX"),
                                        row("suma_asegurada"), row("prima_neta"), row("prima_inc"), row("prima_fhm"), row("prima_tev"),
                                        row("prima_rc"), row("prima_casco"), row("prima_guerra"))
            Next
        End If

        Funciones.LlenaGrid(gvd_Distribucion, (From c In dtDistribucion.AsEnumerable() Where c.Field(Of Integer)("cod_grupo") = Grupo Select c).CopyToDataTable)

        For Each row In gvd_Distribucion.Rows
            CType(row.FindControl("txt_Descripcion"), TextBox).Attributes("style") = "background-color: #3B6882;color: White;"

            If row.rowIndex = 0 Then
                CType(row.FindControl("chk_Sel"), CheckBox).Visible = False
                CType(row.FindControl("txt_Descripcion"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrcPart"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrcPartGMX"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_LimResp"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrimaNeta"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrimaINC"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrimaFHM"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrimaTEV"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrimaRC"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrimaCSC"), TextBox).Font.Bold = True
                CType(row.FindControl("txt_PrimaGRA"), TextBox).Font.Bold = True

                CType(row.FindControl("txt_PrcPart"), TextBox).Attributes.Add("style", "display:none")
                CType(row.FindControl("txt_PrcPartGMX"), TextBox).Enabled = False
            End If
        Next
    End Sub

    Private Sub LlenaGridBroker(ByVal bln_Nuevo As Boolean, Optional ByVal Grupo As Integer = 0)

        If bln_Nuevo = True Then
            dtBroker = New DataTable
            dtBroker.Columns.Add("cod_grupo", GetType(Integer))
            dtBroker.Columns.Add("cod_broker", GetType(Integer))
            dtBroker.Columns.Add("Descripcion")
            dtBroker.Columns.Add("prc_partCoas", GetType(Decimal))
            dtBroker.Columns.Add("prc_part", GetType(Decimal))
            dtBroker.Columns.Add("suma_asegurada", GetType(Decimal))
            dtBroker.Columns.Add("prima_neta", GetType(Decimal))
            dtBroker.Columns.Add("prc_com", GetType(Decimal))
            dtBroker.Columns.Add("comision", GetType(Decimal))
            dtBroker.Columns.Add("pnr", GetType(Decimal))
            dtBroker.Columns.Add("prima_inc", GetType(Decimal))
            dtBroker.Columns.Add("prc_com_inc", GetType(Decimal))
            dtBroker.Columns.Add("com_inc", GetType(Decimal))
            dtBroker.Columns.Add("pnr_inc", GetType(Decimal))
            dtBroker.Columns.Add("prima_fhm", GetType(Decimal))
            dtBroker.Columns.Add("prc_com_fhm", GetType(Decimal))
            dtBroker.Columns.Add("com_fhm", GetType(Decimal))
            dtBroker.Columns.Add("pnr_fhm", GetType(Decimal))
            dtBroker.Columns.Add("prima_tev", GetType(Decimal))
            dtBroker.Columns.Add("prc_com_tev", GetType(Decimal))
            dtBroker.Columns.Add("com_tev", GetType(Decimal))
            dtBroker.Columns.Add("pnr_tev", GetType(Decimal))
            dtBroker.Columns.Add("prima_rc", GetType(Decimal))
            dtBroker.Columns.Add("prc_com_rc", GetType(Decimal))
            dtBroker.Columns.Add("com_rc", GetType(Decimal))
            dtBroker.Columns.Add("pnr_rc", GetType(Decimal))
            dtBroker.Columns.Add("prima_casco", GetType(Decimal))
            dtBroker.Columns.Add("prc_com_casco", GetType(Decimal))
            dtBroker.Columns.Add("com_casco", GetType(Decimal))
            dtBroker.Columns.Add("pnr_casco", GetType(Decimal))
            dtBroker.Columns.Add("prima_guerra", GetType(Decimal))
            dtBroker.Columns.Add("prc_com_guerra", GetType(Decimal))
            dtBroker.Columns.Add("com_guerra", GetType(Decimal))
            dtBroker.Columns.Add("pnr_guerra", GetType(Decimal))
            dtBroker.Columns.Add("prc_corretaje", GetType(Decimal))
            dtBroker.Columns.Add("corretaje", GetType(Decimal))
            dtBroker.Columns.Add("estatus")
            dtBroker.Columns.Add("observaciones")
        End If


        Dim qry = From c In dtBroker.AsEnumerable() Where c.Field(Of Integer)("cod_grupo") = Grupo Select c

        If qry.Count > 0 Then
            Funciones.LlenaGrid(gvd_Intermediario, qry.CopyToDataTable())
        Else
            Funciones.LlenaGrid(gvd_Intermediario, Nothing)
        End If

        For Each row In gvd_Intermediario.Rows
            If TryCast(row.FindControl("lnk_Clave"), LinkButton).Text = 0 Then
                TryCast(row.FindControl("txt_PrcCorretaje"), TextBox).Attributes.Add("style", "display:none")
                TryCast(row.FindControl("txt_Corretaje"), TextBox).Attributes.Add("style", "display:none")
                TryCast(row.FindControl("ddl_Estatus"), DropDownList).Attributes.Add("style", "display:none")
            Else
                TryCast(row.FindControl("ddl_Estatus"), DropDownList).SelectedValue = gvd_Intermediario.DataKeys(row.RowIndex)("Estatus")
                Estado_Fila(gvd_Intermediario, row.rowIndex, gvd_Intermediario.DataKeys(row.RowIndex)("Estatus"))
            End If
        Next
    End Sub

    Private Sub LlenaGridReasegurador(ByVal bln_Nuevo As Boolean, Optional ByVal Grupo As Integer = 0, Optional ByVal cod_broker As Integer = 0)

        If bln_Nuevo = True Then
            dtReasegurador = New DataTable
            dtReasegurador.Columns.Add("cod_grupo", GetType(Integer))
            dtReasegurador.Columns.Add("cod_broker", GetType(Integer))
            dtReasegurador.Columns.Add("cod_cia", GetType(Integer))
            dtReasegurador.Columns.Add("Descripcion")
            dtReasegurador.Columns.Add("cod_naic")
            dtReasegurador.Columns.Add("calif")
            dtReasegurador.Columns.Add("age_Calif")
            dtReasegurador.Columns.Add("prc_part100", GetType(Decimal))
            dtReasegurador.Columns.Add("prc_part", GetType(Decimal))
            dtReasegurador.Columns.Add("suma_asegurada", GetType(Decimal))
            dtReasegurador.Columns.Add("prima_neta", GetType(Decimal))
            dtReasegurador.Columns.Add("prc_com", GetType(Decimal))
            dtReasegurador.Columns.Add("comision", GetType(Decimal))
            dtReasegurador.Columns.Add("pnr", GetType(Decimal))
            dtReasegurador.Columns.Add("prima_inc", GetType(Decimal))
            dtReasegurador.Columns.Add("prc_com_inc", GetType(Decimal))
            dtReasegurador.Columns.Add("com_inc", GetType(Decimal))
            dtReasegurador.Columns.Add("pnr_inc", GetType(Decimal))
            dtReasegurador.Columns.Add("prima_fhm", GetType(Decimal))
            dtReasegurador.Columns.Add("prc_com_fhm", GetType(Decimal))
            dtReasegurador.Columns.Add("com_fhm", GetType(Decimal))
            dtReasegurador.Columns.Add("pnr_fhm", GetType(Decimal))
            dtReasegurador.Columns.Add("prima_tev", GetType(Decimal))
            dtReasegurador.Columns.Add("prc_com_tev", GetType(Decimal))
            dtReasegurador.Columns.Add("com_tev", GetType(Decimal))
            dtReasegurador.Columns.Add("pnr_tev", GetType(Decimal))
            dtReasegurador.Columns.Add("prima_rc", GetType(Decimal))
            dtReasegurador.Columns.Add("prc_com_rc", GetType(Decimal))
            dtReasegurador.Columns.Add("com_rc", GetType(Decimal))
            dtReasegurador.Columns.Add("pnr_rc", GetType(Decimal))
            dtReasegurador.Columns.Add("prima_casco", GetType(Decimal))
            dtReasegurador.Columns.Add("prc_com_casco", GetType(Decimal))
            dtReasegurador.Columns.Add("com_casco", GetType(Decimal))
            dtReasegurador.Columns.Add("pnr_casco", GetType(Decimal))
            dtReasegurador.Columns.Add("prima_guerra", GetType(Decimal))
            dtReasegurador.Columns.Add("prc_com_guerra", GetType(Decimal))
            dtReasegurador.Columns.Add("com_guerra", GetType(Decimal))
            dtReasegurador.Columns.Add("pnr_guerra", GetType(Decimal))
            dtReasegurador.Columns.Add("estatus")
            dtReasegurador.Columns.Add("observaciones")
        End If

        Dim qry = From c In dtReasegurador.AsEnumerable() Where c.Field(Of Integer)("cod_grupo") = Grupo And c.Field(Of Integer)("cod_broker") = cod_broker Select c

        If qry.Count > 0 Then
            Funciones.LlenaGrid(gvd_Reasegurador, qry.CopyToDataTable())
        Else
            Funciones.LlenaGrid(gvd_Reasegurador, Nothing)
        End If

        If hid_IndiceBroker.Value > -1 Then
            If CType(gvd_Intermediario.Rows(hid_IndiceBroker.Value).FindControl("lnk_Clave"), LinkButton).Text > 0 Then
                gvd_Reasegurador.Columns(16).Visible = False

                For Each row In gvd_Reasegurador.Rows
                    TryCast(row.FindControl("ddl_Estatus"), DropDownList).SelectedValue = TryCast(gvd_Intermediario.Rows(hid_IndiceBroker.Value).FindControl("ddl_Estatus"), DropDownList).SelectedValue
                    Estado_Fila(gvd_Reasegurador, row.rowIndex, TryCast(gvd_Intermediario.Rows(hid_IndiceBroker.Value).FindControl("ddl_Estatus"), DropDownList).SelectedValue)
                Next
            Else
                gvd_Reasegurador.Columns(16).Visible = True

                For Each row In gvd_Reasegurador.Rows
                    TryCast(row.FindControl("ddl_Estatus"), DropDownList).SelectedValue = gvd_Reasegurador.DataKeys(row.RowIndex)("Estatus")
                    Estado_Fila(gvd_Reasegurador, row.rowIndex, gvd_Reasegurador.DataKeys(row.RowIndex)("Estatus"))
                Next
            End If
        End If


    End Sub

    Private Sub LlenaGridSubjetividad(ByVal bln_Nuevo As Boolean, ByVal Grupo As Integer, ByVal cod_broker As Integer, Optional ByVal cod_cia As Integer = 0)

        If bln_Nuevo = True Then
            dtSubjetividad = New DataTable
            dtSubjetividad.Columns.Add("cod_grupo", GetType(Integer))
            dtSubjetividad.Columns.Add("cod_broker", GetType(Decimal))
            dtSubjetividad.Columns.Add("cod_cia", GetType(Decimal))
            dtSubjetividad.Columns.Add("Num")
            dtSubjetividad.Columns.Add("Fecha")
            dtSubjetividad.Columns.Add("FechaReal")
            dtSubjetividad.Columns.Add("Subjetividad")
            dtSubjetividad.Columns.Add("sn_Subjetividad")
        End If

        Dim qry = From c In dtSubjetividad.AsEnumerable() Where c.Field(Of Integer)("cod_grupo") = Grupo And c.Field(Of Decimal)("cod_broker") = cod_broker And c.Field(Of Decimal)("cod_cia") = cod_cia Select c

        If qry.Count > 0 Then
            Funciones.LlenaGrid(gvd_Subjetividad, qry.CopyToDataTable())
        Else
            Funciones.LlenaGrid(gvd_Subjetividad, Nothing)
        End If
    End Sub

    Private Sub LlenaGridIncisos(ByVal bln_Nuevo As Boolean)

        If bln_Nuevo = True Then
            dtIncisos = New DataTable
            dtIncisos.Columns.Add("cod_inciso")
            dtIncisos.Columns.Add("cod_ramo")
            dtIncisos.Columns.Add("Ramo")
            dtIncisos.Columns.Add("SubRamo")
            dtIncisos.Columns.Add("Seccion")
            dtIncisos.Columns.Add("Cobertura")
            dtIncisos.Columns.Add("sn_adicional")
            dtIncisos.Columns.Add("valores_totales")
            dtIncisos.Columns.Add("suma_asegurada")
            dtIncisos.Columns.Add("prima_neta")
            dtIncisos.Columns.Add("prima_inc")
            dtIncisos.Columns.Add("prima_fhm")
            dtIncisos.Columns.Add("prima_tev")
            dtIncisos.Columns.Add("prima_rc")
            dtIncisos.Columns.Add("prima_casco")
            dtIncisos.Columns.Add("prima_guerra")
        End If

        Funciones.LlenaGrid(gvd_RiesgosGrupo, dtIncisos)

        If Not bln_Nuevo Then
            ValidaRamosRiesgo(gvd_RiesgosGrupo, -1, {10, 11, 12, 13, 14, 15})
        End If
    End Sub

    Private Sub LlenaGridRepartoResumen(ByVal bln_Nuevo As Boolean, Optional ByVal Grupo As Integer = 0)
        If bln_Nuevo = True Then
            dtRepartoResumen = New DataTable
            dtRepartoResumen.Columns.Add("cod_grupo", GetType(Integer))
            dtRepartoResumen.Columns.Add("Concepto")
            dtRepartoResumen.Columns.Add("Descripcion")
            dtRepartoResumen.Columns.Add("prc_part")
            dtRepartoResumen.Columns.Add("suma_asegurada")
            dtRepartoResumen.Columns.Add("prima_neta")
            dtRepartoResumen.Columns.Add("prima_inc")
            dtRepartoResumen.Columns.Add("prima_fhm")
            dtRepartoResumen.Columns.Add("prima_tev")
            dtRepartoResumen.Columns.Add("prima_rc")
            dtRepartoResumen.Columns.Add("prima_casco")
            dtRepartoResumen.Columns.Add("prima_guerra")
        End If

        If dtRepartoResumen.Select("cod_grupo ='" & Grupo & "'").Length = 0 Then
            dtRepartoResumen.Rows.Add(Grupo, "TOTAL", "", "100.00", 0, 0, 0, 0, 0, 0, 0, 0)
            dtRepartoResumen.Rows.Add(Grupo, "", "GMX", "100.0000", 0, 0, 0, 0, 0, 0, 0, 0)


            dtRepartoResumen.Rows.Add(Grupo, "", "RETENCION", "0.0000", 0, 0, 0, 0, 0, 0, 0, 0)
            dtRepartoResumen.Rows.Add(Grupo, "Contrato 1", "CUOTA PARTE", "0.0000", 0, 0, 0, 0, 0, 0, 0, 0)
            dtRepartoResumen.Rows.Add(Grupo, "Contrato 3", "1ER EXCEDENTE", "0.0000", 0, 0, 0, 0, 0, 0, 0, 0)
            dtRepartoResumen.Rows.Add(Grupo, "Contrato 2", "2DO EXCEDENTE", "0.0000", 0, 0, 0, 0, 0, 0, 0, 0)
            dtRepartoResumen.Rows.Add(Grupo, "", "FACULTATIVO", "0.0000", 0, 0, 0, 0, 0, 0, 0, 0)
            dtRepartoResumen.Rows.Add(Grupo, "", "TOTAL", "0.0000", 0, 0, 0, 0, 0, 0, 0, 0)
        End If

        Funciones.LlenaGrid(gvd_RepartoResumen, (From c In dtRepartoResumen.AsEnumerable() Where c.Field(Of Integer)("cod_grupo") = Grupo Select c).CopyToDataTable)
    End Sub

    Private Sub LlenaGridDistribucionResumen(ByVal bln_Nuevo As Boolean, Optional ByVal Grupo As Integer = 0)

        If bln_Nuevo = True Then
            dtDistribucionResumen = New DataTable
            dtDistribucionResumen.Columns.Add("cod_grupo", GetType(Integer))
            dtDistribucionResumen.Columns.Add("Reasegurador")
            dtDistribucionResumen.Columns.Add("Corredor")
            dtDistribucionResumen.Columns.Add("reg_cnsf")
            dtDistribucionResumen.Columns.Add("prc_part")
            dtDistribucionResumen.Columns.Add("suma_asegurada")
            dtDistribucionResumen.Columns.Add("prima_neta")
            dtDistribucionResumen.Columns.Add("prima_inc")
            dtDistribucionResumen.Columns.Add("prima_fhm")
            dtDistribucionResumen.Columns.Add("prima_tev")
            dtDistribucionResumen.Columns.Add("prima_rc")
            dtDistribucionResumen.Columns.Add("prima_casco")
            dtDistribucionResumen.Columns.Add("prima_guerra")
            dtDistribucionResumen.Columns.Add("prc_corretaje")
            dtDistribucionResumen.Columns.Add("corretaje")
            dtDistribucionResumen.Columns.Add("prc_com_inc")
            dtDistribucionResumen.Columns.Add("prc_com_fhm")
            dtDistribucionResumen.Columns.Add("prc_com_tev")
            dtDistribucionResumen.Columns.Add("prc_com_rc")
            dtDistribucionResumen.Columns.Add("prc_com_casco")
            dtDistribucionResumen.Columns.Add("prc_com_guerra")
            dtDistribucionResumen.Columns.Add("pnr")
        End If

        If dtDistribucionResumen.Select("cod_grupo ='" & Grupo & "'").Length = 0 Then
            dtDistribucionResumen.Rows.Add(Grupo, "Reasegurador", "Corredor", "CNSF", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
        End If

        Dim qry = From c In dtDistribucionResumen.AsEnumerable() Where c.Field(Of Integer)("cod_grupo") = Grupo Select c

        If qry.Count > 0 Then
            Funciones.LlenaGrid(gvd_DistribucionResumen, qry.CopyToDataTable())
        Else
            Funciones.LlenaGrid(gvd_DistribucionResumen, Nothing)
        End If

    End Sub

    Private Sub LlenaGridComisionResumen(ByVal bln_Nuevo As Boolean, Optional ByVal Grupo As Integer = 0)
        If bln_Nuevo = True Then
            dtComisionResumen = New DataTable
            dtComisionResumen.Columns.Add("cod_grupo", GetType(Integer))
            dtComisionResumen.Columns.Add("ramo")
            dtComisionResumen.Columns.Add("prc_com")
            dtComisionResumen.Columns.Add("comision")
        End If

        If dtComisionResumen.Select("cod_grupo ='" & Grupo & "'").Length = 0 Then
            dtComisionResumen.Rows.Add(Grupo, "INCENDIO", 0, 0)
            dtComisionResumen.Rows.Add(Grupo, "F.H.M.", 0, 0)
            dtComisionResumen.Rows.Add(Grupo, "T.E.V.", 0, 0)
        End If

        Funciones.LlenaGrid(gvd_ComisionResumen, (From c In dtComisionResumen.AsEnumerable() Where c.Field(Of Integer)("cod_grupo") = Grupo Select c).CopyToDataTable)

    End Sub

    Private Function ObtieneSeleccionados(ByRef gvd_Control As GridView, ByVal chk_Control As String, ByVal Clave As String, Optional ByVal Checked As Boolean = True) As String
        Dim strSel = "-1"
        For Each row In gvd_Control.Rows
            Dim chk_Sel As CheckBox = TryCast(row.FindControl(chk_Control), CheckBox)
            If chk_Sel.Checked = Checked And chk_Sel.Enabled = True Then
                strSel = strSel & "," & gvd_Control.DataKeys(row.RowIndex)(Clave)
            End If
        Next
        strSel = Replace(strSel, "-1,", "")
        ObtieneSeleccionados = IIf(strSel = "-1", vbNullString, strSel)
    End Function

    Private Sub btn_AgregaFila_Click(sender As Object, e As EventArgs) Handles btn_AgregaFila.Click
        Try
            Dim inciso As Integer = 1

            fn_ActualizaDataRiesgo()

            If dtRiesgo.Rows.Count > 0 Then
                inciso = gvd_Riesgo.DataKeys(gvd_Riesgo.Rows.Count - 1)("cod_inciso") + 1
            End If

            hid_SelCob.Value = ""
            For Each row In gvd_Cobertura.Rows
                Dim txt_SelCob As TextBox = TryCast(row.FindControl("txt_SelCob"), TextBox)

                If txt_SelCob.Text = "1" Then
                    Dim lbl_Clave As Label = TryCast(row.FindControl("lbl_Clave"), Label)
                    Dim lbl_Descripcion As Label = TryCast(row.FindControl("lbl_Descripcion"), Label)
                    hid_SelCob.Value = hid_SelCob.Value & "|" & lbl_Clave.Text & "~" & lbl_Descripcion.Text
                End If
            Next

            hid_SelRie.Value = ""
            If hid_SelCob.Value = "" Then
                For Each row In gvd_Seccion.Rows
                    Dim txt_SelRie As TextBox = TryCast(row.FindControl("txt_SelRie"), TextBox)
                    If txt_SelRie.Text = "1" Then
                        Dim lbl_Clave As Label = TryCast(row.FindControl("lbl_Clave"), Label)
                        Dim lbl_Descripcion As Label = TryCast(row.FindControl("lbl_Descripcion"), Label)
                        hid_SelRie.Value = hid_SelRie.Value & "|" & lbl_Clave.Text & "~" & lbl_Descripcion.Text
                    End If
                Next
            Else
                If CType(gvd_Seccion.Rows(hid_Seccion.Value).FindControl("txt_SelRie"), TextBox).Text = "1" Then
                    hid_SelRie.Value = hid_SelRie.Value & "|" & CType(gvd_Seccion.Rows(hid_Seccion.Value).FindControl("lbl_Clave"), Label).Text & "~" &
                                                                CType(gvd_Seccion.Rows(hid_Seccion.Value).FindControl("lbl_Descripcion"), Label).Text
                End If
            End If

            hid_SelRam.Value = ""
            If hid_SelRie.Value = "" Then
                For Each row In gvd_RamoSbr.Rows
                    Dim txt_SelRam As TextBox = TryCast(row.FindControl("txt_SelRam"), TextBox)
                    If txt_SelRam.Text = "1" Then
                        Dim lbl_ClaveRamo As Label = TryCast(row.FindControl("lbl_ClaveRamo"), Label)
                        Dim lbl_DescRamo As Label = TryCast(row.FindControl("lbl_DescRamo"), Label)
                        Dim lbl_ClaveSubramo As Label = TryCast(row.FindControl("lbl_ClaveSubramo"), Label)
                        Dim lbl_DescSubramo As Label = TryCast(row.FindControl("lbl_DescSubramo"), Label)

                        hid_SelRam.Value = hid_SelRam.Value & "|" & lbl_ClaveRamo.Text & "~" & lbl_DescRamo.Text & "~" & lbl_ClaveSubramo.Text & "~" & lbl_DescSubramo.Text
                    End If
                Next
            Else
                If CType(gvd_RamoSbr.Rows(hid_Ramo.Value).FindControl("txt_SelRam"), TextBox).Text = "1" Then
                    hid_SelRam.Value = hid_SelRam.Value & "|" & CType(gvd_RamoSbr.Rows(hid_Ramo.Value).FindControl("lbl_ClaveRamo"), Label).Text & "~" &
                                                                CType(gvd_RamoSbr.Rows(hid_Ramo.Value).FindControl("lbl_DescRamo"), Label).Text & "~" &
                                                                CType(gvd_RamoSbr.Rows(hid_Ramo.Value).FindControl("lbl_ClaveSubramo"), Label).Text & "~" &
                                                                CType(gvd_RamoSbr.Rows(hid_Ramo.Value).FindControl("lbl_DescSubramo"), Label).Text
                End If
            End If

            Dim Ramos() As String = Split(hid_SelRam.Value, "|")
            Dim Secciones() As String = Split(hid_SelRie.Value, "|")
            Dim Coberturas() As String = Split(hid_SelCob.Value, "|")

            If hid_SelRam.Value <> "" Then

                If dtRiesgo.Rows.Count = 0 Then
                    dtRiesgo.Rows.Add(0, 0, "0", "TOTAL", "", "", "", "", "", "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
                End If

                If Len(hid_SelRie.Value) = 0 Then
                    For Each ramo In Ramos
                        If ramo <> "" Then
                            dtRiesgo.Rows.Add(0, inciso, Split(ramo, "~")(0), Split(ramo, "~")(1), Split(ramo, "~")(2), Split(ramo, "~")(3), "", "", "", "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
                            inciso = inciso + 1
                        End If
                    Next
                Else
                    If Len(hid_SelCob.Value) = 0 Then
                        For Each seccion In Secciones
                            If seccion <> "" Then
                                dtRiesgo.Rows.Add(0, inciso, Split(Ramos(1), "~")(0), Split(Ramos(1), "~")(1), Split(Ramos(1), "~")(2), Split(Ramos(1), "~")(3), Split(seccion, "~")(0), Split(seccion, "~")(1), "", "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
                                inciso = inciso + 1
                            End If
                        Next
                    Else
                        For Each cobertura In Coberturas
                            If cobertura <> "" Then
                                dtRiesgo.Rows.Add(0, inciso, Split(Ramos(1), "~")(0), Split(Ramos(1), "~")(1), Split(Ramos(1), "~")(2), Split(Ramos(1), "~")(3), Split(Secciones(1), "~")(0), Split(Secciones(1), "~")(1), Split(cobertura, "~")(0), Split(cobertura, "~")(1), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
                                inciso = inciso + 1
                            End If
                        Next
                    End If
                End If
            End If

            For Each row In gvd_RamoSbr.Rows
                TryCast(row.FindControl("txt_SelRam"), TextBox).Text = "0"
            Next

            Funciones.LlenaGrid(gvd_Seccion, Nothing)
            Funciones.LlenaGrid(gvd_Cobertura, Nothing)


            LlenaGridRiesgo(False)
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub


    Private Sub btn_RemoveRiesgo_Click(sender As Object, e As EventArgs) Handles btn_RemoveRiesgo.Click
        Try
            Dim strSel As String = ObtieneSeleccionados(gvd_Riesgo, "chk_Riesgo", "cod_inciso")

            If Len(strSel) > 0 Then
                fn_ActualizaDataRiesgo()

                Dim myRow() As Data.DataRow
                myRow = dtRiesgo.Select("cod_inciso IN (" & strSel & ")")

                For Each item In myRow
                    item.Delete()
                Next
                dtRiesgo.AcceptChanges()

                LlenaGridRiesgo(False)
            Else
                Mensaje.MuestraMensaje("Mesa de Control", "No se ha seleccionado ningun elemento", TipoMsg.Advertencia)
            End If

        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub LiberaRiesgos(ByVal strRiesgos As String)
        Dim Riesgos() As String = Split(strRiesgos, ",")
        For Each row In gvd_Riesgo.Rows
            If Riesgos.Contains(TryCast(row.FindControl("lbl_Inciso"), Label).Text) Then
                TryCast(row.FindControl("chk_Riesgo"), CheckBox).Enabled = True
                TryCast(row.FindControl("chk_Riesgo"), CheckBox).Checked = False
                TryCast(row.FindControl("opt_Facultativo"), RadioButtonList).Enabled = True
            End If
        Next
    End Sub

    Private Sub btn_RemoveGrupo_Click(sender As Object, e As EventArgs) Handles btn_RemoveGrupo.Click
        Try
            Dim strSel = ObtieneSeleccionados(gvd_Agrupacion, "chk_Sel", "cod_grupo")

            If Len(strSel) > 0 Then
                fn_ActualizaDataGrupo()

                Dim myRow() As Data.DataRow
                myRow = dtAgrupacion.Select("cod_grupo IN (" & strSel & ")")

                For Each item In myRow
                    Quita_Reparto(item("cod_grupo"), vbNullString)
                    Quita_Intermediario(item("cod_grupo"), vbNullString)
                    LiberaRiesgos(item("incisos"))
                    item.Delete()
                Next
                dtAgrupacion.AcceptChanges()

                LlenaGridAgrupacion(False)

                If gvd_Agrupacion.Rows.Count > 0 Then
                    hid_IndiceGrupo.Value = 0
                    SeleccionaAgrupacion(hid_IndiceGrupo.Value, gvd_Agrupacion.Rows.Count - 1)
                Else
                    hid_IndiceGrupo.Value = -1
                    hid_IndiceGrupoDet.Value = -1
                    hid_IndiceBroker.Value = -1
                    hid_IndiceReas.Value = -1

                    lbl_Distribucion.Text = vbNullString
                    lbl_Intermediarios.Text = vbNullString
                    lbl_Reaseguradores.Text = vbNullString
                    lbl_Comisiones.Text = vbNullString
                    lbl_Subjetividades.Text = vbNullString
                    txt_Observaciones.Text = vbNullString

                    'Llena Información de Reparto
                    LlenaGridReparto(False, 0)

                    'Llena Información de Distribución
                    LlenaGridDistribucion(False, 0)

                    'Llena Información de Broker
                    LlenaGridBroker(False, 0)

                    'Llena Información de Broker
                    LlenaGridReasegurador(False, 0)

                    'Llena Información de Broker
                    LlenaGridSubjetividad(False, 0, 0)

                    Funciones.EjecutaFuncion("fn_CerrarModalSimple('#Subjetividad');", "CierraSubjetividad")
                    Funciones.EjecutaFuncion("fn_CerrarModalSimple('#Comisiones')", "CierraComisiones")
                End If
            Else
                Mensaje.MuestraMensaje("Mesa de Control", "No se ha seleccionado ningun elemento", TipoMsg.Advertencia)
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub Quita_Reparto(ByVal Grupo As Integer, ByVal cod_reparto As String)
        Dim myRow() As Data.DataRow
        Dim strConsulta As String = vbNullString

        fn_ActualizaDataReparto(Grupo)

        If Len(cod_reparto) > 0 Then
            strConsulta = " AND cod_reparto IN (" & cod_reparto & ")"
        End If

        myRow = dtReparto.Select("cod_grupo = " & Grupo & strConsulta)

        For Each item In myRow
            item.Delete()
        Next
        dtReparto.AcceptChanges()
    End Sub

    Private Sub Quita_Capa(ByVal Grupo As Integer, ByVal cod_capa As String)
        Dim myRow() As Data.DataRow
        Dim strConsulta As String = vbNullString

        fn_ActualizaDataCapa(Grupo)

        If Len(cod_capa) > 0 Then
            strConsulta = " AND cod_capa IN (" & cod_capa & ")"
        End If

        myRow = dtCapas.Select("cod_grupo = " & Grupo & strConsulta)

        For Each item In myRow
            item.Delete()
        Next
        dtCapas.AcceptChanges()
    End Sub

    Private Sub Renumera_Capa(ByVal Grupo As Integer)
        Dim Capa As Integer = 1
        Dim myRow() As Data.DataRow
        Dim strConsulta As String = vbNullString

        myRow = dtCapas.Select("cod_grupo = " & Grupo)

        For Each item In myRow
            item("cod_capa") = Capa
            Capa = Capa + 1
        Next

    End Sub

    Private Sub Quita_Intermediario(ByVal Grupo As Integer, ByVal cod_brokers As String)
        Dim myRow() As Data.DataRow
        Dim strConsulta As String = vbNullString

        fn_ActualizaDataBroker(Grupo)

        If Len(cod_brokers) > 0 Then
            strConsulta = " AND cod_broker IN (" & cod_brokers & ")"
        End If

        myRow = dtBroker.Select("cod_grupo = " & Grupo & strConsulta)

        For Each item In myRow
            item.Delete()
        Next
        dtBroker.AcceptChanges()

        Quita_Reasegurador(Grupo, cod_brokers, vbNullString)
        Quita_Subjetividad(Grupo, cod_brokers, vbNullString)
    End Sub

    Private Sub Quita_Reasegurador(ByVal Grupo As Integer, ByVal cod_brokers As String, ByVal Reaseguradores As String)
        Dim myRow() As Data.DataRow
        Dim strConsulta As String = vbNullString

        fn_ActualizaDataReasegurador(Grupo, cod_brokers)

        If Len(cod_brokers) > 0 Then
            strConsulta = " AND cod_broker IN (" & cod_brokers & ")"
        End If

        If Len(Reaseguradores) > 0 Then
            strConsulta = strConsulta & " AND cod_cia IN (" & Reaseguradores & ")"
        End If

        myRow = dtReasegurador.Select("cod_grupo = " & Grupo & strConsulta)

        For Each item In myRow
            item.Delete()
        Next
        dtReasegurador.AcceptChanges()

        Quita_Subjetividad(Grupo, cod_brokers, vbNullString)

    End Sub

    Private Sub Quita_Subjetividad(ByVal Grupo As Integer, ByVal cod_brokers As String, ByVal cod_cias As String, Optional ByVal strNum As String = vbNullString)
        Dim myRow() As Data.DataRow
        Dim strConsulta As String = vbNullString

        fn_ActualizaDataSubjetividad(Grupo, cod_brokers, cod_cias)

        If Len(cod_brokers) > 0 Then
            strConsulta = " AND cod_broker IN (" & cod_brokers & ")"
        End If

        If Len(cod_cias) > 0 Then
            strConsulta = strConsulta & " AND cod_cia IN (" & cod_cias & ")"
        End If

        If Len(strNum) > 0 Then
            strConsulta = strConsulta & " AND Num IN (" & strNum & ")"
        End If

        myRow = dtSubjetividad.Select("cod_grupo = " & Grupo & strConsulta)

        For Each item In myRow
            item.Delete()
        Next
        dtSubjetividad.AcceptChanges()
    End Sub

    Private Function fn_ActualizaDataRiesgo() As DataTable


        For Each row In gvd_Riesgo.Rows
            Dim chk_Riesgo As CheckBox = TryCast(row.FindControl("chk_Riesgo"), CheckBox)
            Dim txt_ClaveRamo As TextBox = TryCast(row.FindControl("txt_ClaveRamo"), TextBox)
            Dim txt_SearchRamo As TextBox = TryCast(row.FindControl("txt_SearchRamo"), TextBox)
            Dim txt_ClaveSubramo As TextBox = TryCast(row.FindControl("txt_ClaveSubramo"), TextBox)
            Dim txt_SearchSubramo As TextBox = TryCast(row.FindControl("txt_SearchSubramo"), TextBox)
            Dim txt_ClaveSeccion As TextBox = TryCast(row.FindControl("txt_ClaveSeccion"), TextBox)
            Dim txt_SearchSeccion As TextBox = TryCast(row.FindControl("txt_SearchSeccion"), TextBox)
            Dim txt_ClaveCobertura As TextBox = TryCast(row.FindControl("txt_ClaveCobertura"), TextBox)
            Dim txt_SearchCobertura As TextBox = TryCast(row.FindControl("txt_SearchCobertura"), TextBox)
            Dim opt_Facultativo As RadioButtonList = TryCast(row.FindControl("opt_Facultativo"), RadioButtonList)
            Dim opt_Adicional As RadioButtonList = TryCast(row.FindControl("opt_Adicional"), RadioButtonList)

            Dim txt_ValoresTotales As TextBox = TryCast(row.FindControl("txt_ValoresTotalesAux"), TextBox)
            Dim txt_LimResp As TextBox = TryCast(row.FindControl("txt_LimRespAux"), TextBox)
            Dim txt_PrimaNeta As TextBox = TryCast(row.FindControl("txt_PrimaNetaAux"), TextBox)

            Dim txt_PrimaINC As TextBox = TryCast(row.FindControl("txt_PrimaINCAux"), TextBox)
            Dim txt_PrimaTEV As TextBox = TryCast(row.FindControl("txt_PrimaTEVAux"), TextBox)
            Dim txt_PrimaFHM As TextBox = TryCast(row.FindControl("txt_PrimaFHMAux"), TextBox)
            Dim txt_PrimaRC As TextBox = TryCast(row.FindControl("txt_PrimaRCAux"), TextBox)
            Dim txt_PrimaCSC As TextBox = TryCast(row.FindControl("txt_PrimaCSCAux"), TextBox)
            Dim txt_PrimaGRA As TextBox = TryCast(row.FindControl("txt_PrimaGRAAux"), TextBox)

            Dim txt_PrcComAge As TextBox = TryCast(row.FindControl("txt_PrcComAge"), TextBox)
            Dim txt_ComAge As TextBox = TryCast(row.FindControl("txt_ComAgeAux"), TextBox)
            Dim txt_PrcComAdiAge As TextBox = TryCast(row.FindControl("txt_PrcComAdiAge"), TextBox)
            Dim txt_ComAdiAge As TextBox = TryCast(row.FindControl("txt_ComAdiAgeAux"), TextBox)
            Dim txt_Cuota As TextBox = TryCast(row.FindControl("txt_Cuota"), TextBox)

            Dim txt_PrcFeeGmx As TextBox = TryCast(row.FindControl("txt_PrcFeeGmx"), TextBox)
            Dim txt_FeeGmx As TextBox = TryCast(row.FindControl("txt_FeeGmxAux"), TextBox)
            Dim txt_PrcComFac As TextBox = TryCast(row.FindControl("txt_PrcComFac"), TextBox)
            Dim txt_ComFac As TextBox = TryCast(row.FindControl("txt_ComFacAux"), TextBox)

            Dim myRow() As Data.DataRow
            myRow = dtRiesgo.Select("cod_inciso ='" & gvd_Riesgo.DataKeys(row.RowIndex)("cod_inciso") & "'")

            If myRow.Length > 0 Then
                myRow(0)("sn_seleccionado") = IIf(chk_Riesgo.Enabled = False, 1, 0)
                myRow(0)("cod_ramo") = txt_ClaveRamo.Text
                myRow(0)("ramo_desc") = txt_SearchRamo.Text
                myRow(0)("cod_subramo") = txt_ClaveSubramo.Text
                myRow(0)("subramo_desc") = txt_SearchSubramo.Text
                myRow(0)("cod_riesgo") = txt_ClaveSeccion.Text
                myRow(0)("riesgo_desc") = txt_SearchSeccion.Text
                myRow(0)("cod_ind_cob") = txt_ClaveCobertura.Text
                myRow(0)("cobertura_desc") = txt_SearchCobertura.Text
                myRow(0)("sn_facultativo") = opt_Facultativo.SelectedValue
                myRow(0)("sn_adicional") = opt_Adicional.SelectedValue
                myRow(0)("valores_totales") = IIf(txt_ValoresTotales.Text = vbNullString, 0, txt_ValoresTotales.Text)
                myRow(0)("suma_asegurada") = IIf(txt_LimResp.Text = vbNullString, 0, txt_LimResp.Text)
                myRow(0)("prima_neta") = IIf(txt_PrimaNeta.Text = vbNullString, 0, txt_PrimaNeta.Text)
                myRow(0)("prima_inc") = IIf(txt_PrimaINC.Text = vbNullString, 0, txt_PrimaINC.Text)
                myRow(0)("prima_tev") = IIf(txt_PrimaTEV.Text = vbNullString, 0, txt_PrimaTEV.Text)
                myRow(0)("prima_fhm") = IIf(txt_PrimaFHM.Text = vbNullString, 0, txt_PrimaFHM.Text)
                myRow(0)("prima_rc") = IIf(txt_PrimaRC.Text = vbNullString, 0, txt_PrimaRC.Text)
                myRow(0)("prima_casco") = IIf(txt_PrimaCSC.Text = vbNullString, 0, txt_PrimaCSC.Text)
                myRow(0)("prima_guerra") = IIf(txt_PrimaGRA.Text = vbNullString, 0, txt_PrimaGRA.Text)
                myRow(0)("prc_com_age") = IIf(txt_PrcComAge.Text = vbNullString, 0, txt_PrcComAge.Text)
                myRow(0)("com_agente") = IIf(txt_ComAge.Text = vbNullString, 0, txt_ComAge.Text)
                myRow(0)("prc_com_adi_age") = IIf(txt_PrcComAdiAge.Text = vbNullString, 0, txt_PrcComAdiAge.Text)
                myRow(0)("com_adi_agente") = IIf(txt_ComAdiAge.Text = vbNullString, 0, txt_ComAdiAge.Text)
                myRow(0)("cuota") = txt_Cuota.Text
                myRow(0)("prc_FeeGMX") = IIf(txt_PrcFeeGmx.Text = vbNullString, 0, txt_PrcFeeGmx.Text)
                myRow(0)("mnt_FeeGMX") = IIf(txt_FeeGmx.Text = vbNullString, 0, txt_FeeGmx.Text)
                myRow(0)("prc_ComFac") = IIf(txt_PrcComFac.Text = vbNullString, 0, txt_PrcComFac.Text)
                myRow(0)("mnt_ComFac") = IIf(txt_ComFac.Text = vbNullString, 0, txt_ComFac.Text)
            End If
        Next
        Return dtRiesgo
    End Function

    Private Function fn_ActualizaDataGrupo() As DataTable

        For Each row In gvd_Agrupacion.Rows
            Dim txt_Incisos As TextBox = TryCast(row.FindControl("txt_Incisos"), TextBox)
            Dim txt_Ramos As TextBox = TryCast(row.FindControl("txt_Ramos"), TextBox)
            Dim txt_LimResp As TextBox = TryCast(row.FindControl("txt_LimRespAux"), TextBox)
            Dim txt_PrimaNeta As TextBox = TryCast(row.FindControl("txt_PrimaNetaAux"), TextBox)
            Dim txt_PrimaINC As TextBox = TryCast(row.FindControl("txt_PrimaINCAux"), TextBox)
            Dim txt_PrimaFHM As TextBox = TryCast(row.FindControl("txt_PrimaFHMAux"), TextBox)
            Dim txt_PrimaTEV As TextBox = TryCast(row.FindControl("txt_PrimaTEVAux"), TextBox)
            Dim txt_PrimaRC As TextBox = TryCast(row.FindControl("txt_PrimaRCAux"), TextBox)
            Dim txt_PrimaCSC As TextBox = TryCast(row.FindControl("txt_PrimaCSCAux"), TextBox)
            Dim txt_PrimaGRA As TextBox = TryCast(row.FindControl("txt_PrimaGRAAux"), TextBox)

            Dim myRow() As Data.DataRow
            myRow = dtAgrupacion.Select("cod_grupo ='" & gvd_Agrupacion.DataKeys(row.RowIndex)("cod_grupo") & "'")

            If myRow.Length > 0 Then
                myRow(0)("incisos") = txt_Incisos.Text
                myRow(0)("ramos") = txt_Ramos.Text
                myRow(0)("suma_asegurada") = txt_LimResp.Text
                myRow(0)("prima_neta") = txt_PrimaNeta.Text
                myRow(0)("prima_inc") = IIf(txt_PrimaINC.Text = vbNullString, 0, txt_PrimaINC.Text)
                myRow(0)("prima_fhm") = IIf(txt_PrimaFHM.Text = vbNullString, 0, txt_PrimaFHM.Text)
                myRow(0)("prima_tev") = IIf(txt_PrimaTEV.Text = vbNullString, 0, txt_PrimaTEV.Text)
                myRow(0)("prima_rc") = IIf(txt_PrimaINC.Text = vbNullString, 0, txt_PrimaINC.Text)
                myRow(0)("prima_casco") = IIf(txt_PrimaFHM.Text = vbNullString, 0, txt_PrimaFHM.Text)
                myRow(0)("prima_guerra") = IIf(txt_PrimaTEV.Text = vbNullString, 0, txt_PrimaTEV.Text)
            End If
        Next

        Return dtAgrupacion
    End Function

    Private Function fn_ActualizaDataReparto(ByVal Grupo As Integer) As DataTable
        Dim myRow() As Data.DataRow

        For Each row In gvd_Reparto.Rows
            Dim txt_PrcPart As TextBox = TryCast(row.FindControl("txt_PrcPart"), TextBox)
            Dim chk_Lider As CheckBox = TryCast(row.FindControl("chk_Lider"), CheckBox)
            Dim txt_LimResp As TextBox = TryCast(row.FindControl("txt_LimRespAux"), TextBox)
            Dim txt_PrimaNeta As TextBox = TryCast(row.FindControl("txt_PrimaNetaAux"), TextBox)
            Dim txt_PrimaINC As TextBox = TryCast(row.FindControl("txt_PrimaINCAux"), TextBox)
            Dim txt_PrimaFHM As TextBox = TryCast(row.FindControl("txt_PrimaFHMAux"), TextBox)
            Dim txt_PrimaTEV As TextBox = TryCast(row.FindControl("txt_PrimaTEVAux"), TextBox)
            Dim txt_PrimaRC As TextBox = TryCast(row.FindControl("txt_PrimaRCAux"), TextBox)
            Dim txt_PrimaCSC As TextBox = TryCast(row.FindControl("txt_PrimaCSCAux"), TextBox)
            Dim txt_PrimaGRA As TextBox = TryCast(row.FindControl("txt_PrimaGRAAux"), TextBox)

            myRow = dtReparto.Select("cod_grupo ='" & Grupo & "' AND cod_reparto = '" & gvd_Reparto.DataKeys(row.RowIndex)("cod_reparto") & "'")

            If myRow.Length > 0 Then
                myRow(0)("prc_part") = IIf(txt_PrcPart.Text = vbNullString, 0, txt_PrcPart.Text)
                myRow(0)("sn_lider") = IIf(chk_Lider.Checked, 1, 0)
                myRow(0)("suma_asegurada") = txt_LimResp.Text
                myRow(0)("prima_neta") = txt_PrimaNeta.Text
                myRow(0)("prima_inc") = IIf(txt_PrimaINC.Text = vbNullString, 0, txt_PrimaINC.Text)
                myRow(0)("prima_fhm") = IIf(txt_PrimaFHM.Text = vbNullString, 0, txt_PrimaFHM.Text)
                myRow(0)("prima_tev") = IIf(txt_PrimaTEV.Text = vbNullString, 0, txt_PrimaTEV.Text)
                myRow(0)("prima_rc") = IIf(txt_PrimaRC.Text = vbNullString, 0, txt_PrimaRC.Text)
                myRow(0)("prima_casco") = IIf(txt_PrimaCSC.Text = vbNullString, 0, txt_PrimaCSC.Text)
                myRow(0)("prima_guerra") = IIf(txt_PrimaGRA.Text = vbNullString, 0, txt_PrimaGRA.Text)
            End If
        Next

        Return dtReparto
    End Function

    Private Function fn_ActualizaDataCapa(ByVal Grupo As Integer) As DataTable
        Dim myRow() As Data.DataRow

        For Each row In gvd_Capas.Rows
            Dim txt_PrcPartRET As TextBox = TryCast(row.FindControl("txt_PrcPartRET"), TextBox)
            Dim txt_PrcPart As TextBox = TryCast(row.FindControl("txt_PrcPart"), TextBox)
            Dim txt_PrcPartGMXRET As TextBox = TryCast(row.FindControl("txt_PrcPartGMXRET"), TextBox)
            Dim txt_PrcPartGMX As TextBox = TryCast(row.FindControl("txt_PrcPartGMX"), TextBox)
            Dim txt_LimRespRET As TextBox = TryCast(row.FindControl("txt_LimRespRETAux"), TextBox)
            Dim txt_LimResp As TextBox = TryCast(row.FindControl("txt_LimRespAux"), TextBox)
            Dim txt_PrimaNetaRET As TextBox = TryCast(row.FindControl("txt_PrimaNetaRETAux"), TextBox)
            Dim txt_PrimaNeta As TextBox = TryCast(row.FindControl("txt_PrimaNetaAux"), TextBox)
            Dim txt_PrimaINCRET As TextBox = TryCast(row.FindControl("txt_PrimaINCRETAux"), TextBox)
            Dim txt_PrimaINC As TextBox = TryCast(row.FindControl("txt_PrimaINCAux"), TextBox)
            Dim txt_PrimaFHMRET As TextBox = TryCast(row.FindControl("txt_PrimaFHMRETAux"), TextBox)
            Dim txt_PrimaFHM As TextBox = TryCast(row.FindControl("txt_PrimaFHMAux"), TextBox)
            Dim txt_PrimaTEVRET As TextBox = TryCast(row.FindControl("txt_PrimaTEVRETAux"), TextBox)
            Dim txt_PrimaTEV As TextBox = TryCast(row.FindControl("txt_PrimaTEVAux"), TextBox)
            Dim txt_PrimaRCRET As TextBox = TryCast(row.FindControl("txt_PrimaRCRETAux"), TextBox)
            Dim txt_PrimaRC As TextBox = TryCast(row.FindControl("txt_PrimaRCAux"), TextBox)
            Dim txt_PrimaCSCRET As TextBox = TryCast(row.FindControl("txt_PrimaCSCRETAux"), TextBox)
            Dim txt_PrimaCSC As TextBox = TryCast(row.FindControl("txt_PrimaCSCAux"), TextBox)
            Dim txt_PrimaGRARET As TextBox = TryCast(row.FindControl("txt_PrimaGRARETAux"), TextBox)
            Dim txt_PrimaGRA As TextBox = TryCast(row.FindControl("txt_PrimaGRAAux"), TextBox)

            myRow = dtCapas.Select("cod_grupo ='" & Grupo & "' AND cod_capa = '" & gvd_Capas.DataKeys(row.RowIndex)("cod_capa") & "'")

            If myRow.Length > 0 Then
                myRow(0)("prc_part_ret") = IIf(txt_PrcPartRET.Text = vbNullString, 0, txt_PrcPartRET.Text)
                myRow(0)("prc_part") = IIf(txt_PrcPart.Text = vbNullString, 0, txt_PrcPart.Text)
                myRow(0)("prc_partGMX_ret") = IIf(txt_PrcPartGMXRET.Text = vbNullString, 0, txt_PrcPartGMXRET.Text)
                myRow(0)("prc_partGMX") = IIf(txt_PrcPartGMX.Text = vbNullString, 0, txt_PrcPartGMX.Text)
                myRow(0)("suma_asegurada_ret") = txt_LimRespRET.Text
                myRow(0)("suma_asegurada") = txt_LimResp.Text
                myRow(0)("prima_neta_ret") = txt_PrimaNetaRET.Text
                myRow(0)("prima_neta") = txt_PrimaNeta.Text
                myRow(0)("prima_inc_ret") = IIf(txt_PrimaINCRET.Text = vbNullString, 0, txt_PrimaINCRET.Text)
                myRow(0)("prima_inc") = IIf(txt_PrimaINC.Text = vbNullString, 0, txt_PrimaINC.Text)
                myRow(0)("prima_fhm_ret") = IIf(txt_PrimaFHMRET.Text = vbNullString, 0, txt_PrimaFHMRET.Text)
                myRow(0)("prima_fhm") = IIf(txt_PrimaFHM.Text = vbNullString, 0, txt_PrimaFHM.Text)
                myRow(0)("prima_tev_ret") = IIf(txt_PrimaTEVRET.Text = vbNullString, 0, txt_PrimaTEVRET.Text)
                myRow(0)("prima_tev") = IIf(txt_PrimaTEV.Text = vbNullString, 0, txt_PrimaTEV.Text)
                myRow(0)("prima_rc_ret") = IIf(txt_PrimaRCRET.Text = vbNullString, 0, txt_PrimaRCRET.Text)
                myRow(0)("prima_rc") = IIf(txt_PrimaRC.Text = vbNullString, 0, txt_PrimaRC.Text)
                myRow(0)("prima_casco_ret") = IIf(txt_PrimaCSCRET.Text = vbNullString, 0, txt_PrimaCSCRET.Text)
                myRow(0)("prima_casco") = IIf(txt_PrimaCSC.Text = vbNullString, 0, txt_PrimaCSC.Text)
                myRow(0)("prima_guerra_ret") = IIf(txt_PrimaGRARET.Text = vbNullString, 0, txt_PrimaGRARET.Text)
                myRow(0)("prima_guerra") = IIf(txt_PrimaGRA.Text = vbNullString, 0, txt_PrimaGRA.Text)
            End If
        Next

        Return dtCapas
    End Function


    Private Function fn_ActualizaDataDistribucion(ByVal Grupo As Integer) As DataTable
        Dim myRow() As Data.DataRow

        For Each row In gvd_Distribucion.Rows
            Dim txt_PrcPart As TextBox = TryCast(row.FindControl("txt_PrcPart"), TextBox)
            Dim txt_PrcPartGMX As TextBox = TryCast(row.FindControl("txt_PrcPartGMX"), TextBox)
            Dim txt_LimResp As TextBox = TryCast(row.FindControl("txt_LimRespAux"), TextBox)
            Dim txt_PrimaNeta As TextBox = TryCast(row.FindControl("txt_PrimaNetaAux"), TextBox)
            Dim txt_PrimaINC As TextBox = TryCast(row.FindControl("txt_PrimaINCAux"), TextBox)
            Dim txt_PrimaFHM As TextBox = TryCast(row.FindControl("txt_PrimaFHMAux"), TextBox)
            Dim txt_PrimaTEV As TextBox = TryCast(row.FindControl("txt_PrimaTEVAux"), TextBox)
            Dim txt_PrimaRC As TextBox = TryCast(row.FindControl("txt_PrimaRCAux"), TextBox)
            Dim txt_PrimaCSC As TextBox = TryCast(row.FindControl("txt_PrimaCSCAux"), TextBox)
            Dim txt_PrimaGRA As TextBox = TryCast(row.FindControl("txt_PrimaGRAAux"), TextBox)

            myRow = dtDistribucion.Select("cod_grupo ='" & Grupo & "' AND cod_distribucion = '" & gvd_Distribucion.DataKeys(row.RowIndex)("cod_distribucion") & "'")

            If myRow.Length > 0 Then
                myRow(0)("prc_part") = IIf(txt_PrcPart.Text = vbNullString, 0, txt_PrcPart.Text)
                myRow(0)("prc_partGMX") = IIf(txt_PrcPartGMX.Text = vbNullString, 0, txt_PrcPartGMX.Text)
                myRow(0)("suma_asegurada") = txt_LimResp.Text
                myRow(0)("prima_neta") = txt_PrimaNeta.Text
                myRow(0)("prima_inc") = IIf(txt_PrimaINC.Text = vbNullString, 0, txt_PrimaINC.Text)
                myRow(0)("prima_fhm") = IIf(txt_PrimaFHM.Text = vbNullString, 0, txt_PrimaFHM.Text)
                myRow(0)("prima_tev") = IIf(txt_PrimaTEV.Text = vbNullString, 0, txt_PrimaTEV.Text)
                myRow(0)("prima_rc") = IIf(txt_PrimaRC.Text = vbNullString, 0, txt_PrimaRC.Text)
                myRow(0)("prima_casco") = IIf(txt_PrimaCSC.Text = vbNullString, 0, txt_PrimaCSC.Text)
                myRow(0)("prima_guerra") = IIf(txt_PrimaGRA.Text = vbNullString, 0, txt_PrimaGRA.Text)
            End If
        Next

        Return dtDistribucion
    End Function

    Private Function fn_ActualizaDataBroker(ByVal Grupo As Integer) As DataTable
        Dim myRow() As Data.DataRow

        For Each row In gvd_Intermediario.Rows
            Dim txt_PrcPartCoas As TextBox = TryCast(row.FindControl("txt_PrcPartCoas"), TextBox)
            Dim txt_PrcPart As TextBox = TryCast(row.FindControl("txt_PrcPart"), TextBox)
            Dim txt_LimResp As TextBox = TryCast(row.FindControl("txt_LimRespAux"), TextBox)

            Dim txt_PrimaNeta As TextBox = TryCast(row.FindControl("txt_PrimaNetaAux"), TextBox)
            Dim txt_PrcComNeta As TextBox = TryCast(row.FindControl("txt_PrcComNeta"), TextBox)
            Dim txt_ComNeta As TextBox = TryCast(row.FindControl("txt_ComNeta"), TextBox)
            Dim txt_PnrNeta As TextBox = TryCast(row.FindControl("txt_PnrNeta"), TextBox)

            Dim txt_PrimaINC As TextBox = TryCast(row.FindControl("txt_PrimaINCAux"), TextBox)
            Dim txt_PrcComINC As TextBox = TryCast(row.FindControl("txt_PrcComINC"), TextBox)
            Dim txt_ComINC As TextBox = TryCast(row.FindControl("txt_ComINC"), TextBox)
            Dim txt_PnrINC As TextBox = TryCast(row.FindControl("txt_PnrINC"), TextBox)

            Dim txt_PrimaFHM As TextBox = TryCast(row.FindControl("txt_PrimaFHMAux"), TextBox)
            Dim txt_PrcComFHM As TextBox = TryCast(row.FindControl("txt_PrcComFHM"), TextBox)
            Dim txt_ComFHM As TextBox = TryCast(row.FindControl("txt_ComFHM"), TextBox)
            Dim txt_PnrFHM As TextBox = TryCast(row.FindControl("txt_PnrFHM"), TextBox)

            Dim txt_PrimaTEV As TextBox = TryCast(row.FindControl("txt_PrimaTEVAux"), TextBox)
            Dim txt_PrcComTEV As TextBox = TryCast(row.FindControl("txt_PrcComTEV"), TextBox)
            Dim txt_ComTEV As TextBox = TryCast(row.FindControl("txt_ComTEV"), TextBox)
            Dim txt_PnrTEV As TextBox = TryCast(row.FindControl("txt_PnrTEV"), TextBox)

            Dim txt_PrimaRC As TextBox = TryCast(row.FindControl("txt_PrimaRCAux"), TextBox)
            Dim txt_PrcComRC As TextBox = TryCast(row.FindControl("txt_PrcComRC"), TextBox)
            Dim txt_ComRC As TextBox = TryCast(row.FindControl("txt_ComRC"), TextBox)
            Dim txt_PnrRC As TextBox = TryCast(row.FindControl("txt_PnrRC"), TextBox)

            Dim txt_PrimaCSC As TextBox = TryCast(row.FindControl("txt_PrimaCSCAux"), TextBox)
            Dim txt_PrcComCSC As TextBox = TryCast(row.FindControl("txt_PrcComCSC"), TextBox)
            Dim txt_ComCSC As TextBox = TryCast(row.FindControl("txt_ComCSC"), TextBox)
            Dim txt_PnrCSC As TextBox = TryCast(row.FindControl("txt_PnrCSC"), TextBox)

            Dim txt_PrimaGRA As TextBox = TryCast(row.FindControl("txt_PrimaGRAAux"), TextBox)
            Dim txt_PrcComGRA As TextBox = TryCast(row.FindControl("txt_PrcComGRA"), TextBox)
            Dim txt_ComGRA As TextBox = TryCast(row.FindControl("txt_ComGRA"), TextBox)
            Dim txt_PnrGRA As TextBox = TryCast(row.FindControl("txt_PnrGRA"), TextBox)

            Dim txt_PrcCorretaje As TextBox = TryCast(row.FindControl("txt_PrcCorretaje"), TextBox)
            Dim txt_Corretaje As TextBox = TryCast(row.FindControl("txt_CorretajeAux"), TextBox)

            Dim ddl_Estatus As DropDownList = TryCast(row.FindControl("ddl_Estatus"), DropDownList)
            Dim txt_Observaciones As TextBox = TryCast(row.FindControl("txt_Observaciones"), TextBox)

            myRow = dtBroker.Select("cod_grupo ='" & Grupo & "' AND cod_broker = '" & gvd_Intermediario.DataKeys(row.RowIndex)("cod_broker") & "'")

            If myRow.Length > 0 Then
                myRow(0)("prc_partCoas") = IIf(txt_PrcPartCoas.Text = vbNullString, 0, txt_PrcPartCoas.Text)
                myRow(0)("prc_part") = IIf(txt_PrcPart.Text = vbNullString, 0, txt_PrcPart.Text)
                myRow(0)("suma_asegurada") = txt_LimResp.Text
                myRow(0)("prima_neta") = txt_PrimaNeta.Text
                myRow(0)("prc_com") = txt_PrcComNeta.Text
                myRow(0)("comision") = txt_ComNeta.Text
                myRow(0)("pnr") = txt_PnrNeta.Text
                myRow(0)("prima_inc") = IIf(txt_PrimaINC.Text = vbNullString, 0, txt_PrimaINC.Text)
                myRow(0)("prc_com_inc") = IIf(txt_PrcComINC.Text = vbNullString, 0, txt_PrcComINC.Text)
                myRow(0)("com_inc") = IIf(txt_ComINC.Text = vbNullString, 0, txt_ComINC.Text)
                myRow(0)("pnr_inc") = IIf(txt_PnrINC.Text = vbNullString, 0, txt_PnrINC.Text)
                myRow(0)("prima_fhm") = IIf(txt_PrimaFHM.Text = vbNullString, 0, txt_PrimaFHM.Text)
                myRow(0)("prc_com_fhm") = IIf(txt_PrcComFHM.Text = vbNullString, 0, txt_PrcComFHM.Text)
                myRow(0)("com_fhm") = IIf(txt_ComFHM.Text = vbNullString, 0, txt_ComFHM.Text)
                myRow(0)("pnr_fhm") = IIf(txt_PnrFHM.Text = vbNullString, 0, txt_PnrFHM.Text)
                myRow(0)("prima_tev") = IIf(txt_PrimaTEV.Text = vbNullString, 0, txt_PrimaTEV.Text)
                myRow(0)("prc_com_tev") = IIf(txt_PrcComTEV.Text = vbNullString, 0, txt_PrcComTEV.Text)
                myRow(0)("com_tev") = IIf(txt_ComTEV.Text = vbNullString, 0, txt_ComTEV.Text)
                myRow(0)("pnr_tev") = IIf(txt_PnrTEV.Text = vbNullString, 0, txt_PnrTEV.Text)
                myRow(0)("prima_rc") = IIf(txt_PrimaRC.Text = vbNullString, 0, txt_PrimaRC.Text)
                myRow(0)("prc_com_rc") = IIf(txt_PrcComRC.Text = vbNullString, 0, txt_PrcComRC.Text)
                myRow(0)("com_rc") = IIf(txt_ComRC.Text = vbNullString, 0, txt_ComRC.Text)
                myRow(0)("pnr_rc") = IIf(txt_PnrRC.Text = vbNullString, 0, txt_PnrRC.Text)
                myRow(0)("prima_casco") = IIf(txt_PrimaCSC.Text = vbNullString, 0, txt_PrimaCSC.Text)
                myRow(0)("prc_com_casco") = IIf(txt_PrcComCSC.Text = vbNullString, 0, txt_PrcComCSC.Text)
                myRow(0)("com_casco") = IIf(txt_ComCSC.Text = vbNullString, 0, txt_ComCSC.Text)
                myRow(0)("pnr_casco") = IIf(txt_PnrCSC.Text = vbNullString, 0, txt_PnrCSC.Text)
                myRow(0)("prima_guerra") = IIf(txt_PrimaGRA.Text = vbNullString, 0, txt_PrimaGRA.Text)
                myRow(0)("prc_com_guerra") = IIf(txt_PrcComGRA.Text = vbNullString, 0, txt_PrcComGRA.Text)
                myRow(0)("com_guerra") = IIf(txt_ComGRA.Text = vbNullString, 0, txt_ComGRA.Text)
                myRow(0)("pnr_guerra") = IIf(txt_PnrGRA.Text = vbNullString, 0, txt_PnrGRA.Text)
                myRow(0)("prc_corretaje") = IIf(txt_PrcCorretaje.Text = vbNullString, 0, txt_PrcCorretaje.Text)
                myRow(0)("corretaje") = IIf(txt_Corretaje.Text = vbNullString, 0, txt_Corretaje.Text)
                myRow(0)("estatus") = ddl_Estatus.SelectedValue
                myRow(0)("observaciones") = txt_Observaciones.Text
            End If
        Next

        Return dtBroker
    End Function

    Private Function fn_ActualizaDataReasegurador(ByVal Grupo As Integer, ByVal cod_broker As Integer) As DataTable
        Dim myRow() As Data.DataRow

        For Each row In gvd_Reasegurador.Rows
            Dim txt_PrcPart100 As TextBox = TryCast(row.FindControl("txt_PrcPart100"), TextBox)
            Dim txt_PrcPart As TextBox = TryCast(row.FindControl("txt_PrcPart"), TextBox)
            Dim txt_LimResp As TextBox = TryCast(row.FindControl("txt_LimRespAux"), TextBox)

            Dim txt_PrimaNeta As TextBox = TryCast(row.FindControl("txt_PrimaNetaAux"), TextBox)
            Dim txt_PrcComNeta As TextBox = TryCast(row.FindControl("txt_PrcComNeta"), TextBox)
            Dim txt_ComNeta As TextBox = TryCast(row.FindControl("txt_ComNeta"), TextBox)
            Dim txt_PnrNeta As TextBox = TryCast(row.FindControl("txt_PnrNeta"), TextBox)

            Dim txt_PrimaINC As TextBox = TryCast(row.FindControl("txt_PrimaINCAux"), TextBox)
            Dim txt_PrcComINC As TextBox = TryCast(row.FindControl("txt_PrcComINC"), TextBox)
            Dim txt_ComINC As TextBox = TryCast(row.FindControl("txt_ComINC"), TextBox)
            Dim txt_PnrINC As TextBox = TryCast(row.FindControl("txt_PnrINC"), TextBox)

            Dim txt_PrimaFHM As TextBox = TryCast(row.FindControl("txt_PrimaFHMAux"), TextBox)
            Dim txt_PrcComFHM As TextBox = TryCast(row.FindControl("txt_PrcComFHM"), TextBox)
            Dim txt_ComFHM As TextBox = TryCast(row.FindControl("txt_ComFHM"), TextBox)
            Dim txt_PnrFHM As TextBox = TryCast(row.FindControl("txt_PnrFHM"), TextBox)

            Dim txt_PrimaTEV As TextBox = TryCast(row.FindControl("txt_PrimaTEVAux"), TextBox)
            Dim txt_PrcComTEV As TextBox = TryCast(row.FindControl("txt_PrcComTEV"), TextBox)
            Dim txt_ComTEV As TextBox = TryCast(row.FindControl("txt_ComTEV"), TextBox)
            Dim txt_PnrTEV As TextBox = TryCast(row.FindControl("txt_PnrTEV"), TextBox)

            Dim txt_PrimaRC As TextBox = TryCast(row.FindControl("txt_PrimaRCAux"), TextBox)
            Dim txt_PrcComRC As TextBox = TryCast(row.FindControl("txt_PrcComRC"), TextBox)
            Dim txt_ComRC As TextBox = TryCast(row.FindControl("txt_ComRC"), TextBox)
            Dim txt_PnrRC As TextBox = TryCast(row.FindControl("txt_PnrRC"), TextBox)

            Dim txt_PrimaCSC As TextBox = TryCast(row.FindControl("txt_PrimaCSCAux"), TextBox)
            Dim txt_PrcComCSC As TextBox = TryCast(row.FindControl("txt_PrcComCSC"), TextBox)
            Dim txt_ComCSC As TextBox = TryCast(row.FindControl("txt_ComCSC"), TextBox)
            Dim txt_PnrCSC As TextBox = TryCast(row.FindControl("txt_PnrCSC"), TextBox)

            Dim txt_PrimaGRA As TextBox = TryCast(row.FindControl("txt_PrimaGRAAux"), TextBox)
            Dim txt_PrcComGRA As TextBox = TryCast(row.FindControl("txt_PrcComGRA"), TextBox)
            Dim txt_ComGRA As TextBox = TryCast(row.FindControl("txt_ComGRA"), TextBox)
            Dim txt_PnrGRA As TextBox = TryCast(row.FindControl("txt_PnrGRA"), TextBox)

            Dim ddl_Estatus As DropDownList = TryCast(row.FindControl("ddl_Estatus"), DropDownList)
            Dim txt_Observaciones As TextBox = TryCast(row.FindControl("txt_Observaciones"), TextBox)

            myRow = dtReasegurador.Select("cod_grupo ='" & Grupo & "' AND cod_broker = '" & cod_broker & "' AND cod_cia = '" & gvd_Reasegurador.DataKeys(row.RowIndex)("cod_cia") & "'")

            If myRow.Length > 0 Then
                myRow(0)("prc_part100") = IIf(txt_PrcPart100.Text = vbNullString, 0, txt_PrcPart100.Text)
                myRow(0)("prc_part") = IIf(txt_PrcPart.Text = vbNullString, 0, txt_PrcPart.Text)
                myRow(0)("suma_asegurada") = txt_LimResp.Text
                myRow(0)("prima_neta") = txt_PrimaNeta.Text
                myRow(0)("prc_com") = txt_PrcComNeta.Text
                myRow(0)("comision") = txt_ComNeta.Text
                myRow(0)("pnr") = txt_PnrNeta.Text
                myRow(0)("prima_inc") = IIf(txt_PrimaINC.Text = vbNullString, 0, txt_PrimaINC.Text)
                myRow(0)("prc_com_inc") = IIf(txt_PrcComINC.Text = vbNullString, 0, txt_PrcComINC.Text)
                myRow(0)("com_inc") = IIf(txt_ComINC.Text = vbNullString, 0, txt_ComINC.Text)
                myRow(0)("pnr_inc") = IIf(txt_PnrINC.Text = vbNullString, 0, txt_PnrINC.Text)
                myRow(0)("prima_fhm") = IIf(txt_PrimaFHM.Text = vbNullString, 0, txt_PrimaFHM.Text)
                myRow(0)("prc_com_fhm") = IIf(txt_PrcComFHM.Text = vbNullString, 0, txt_PrcComFHM.Text)
                myRow(0)("com_fhm") = IIf(txt_ComFHM.Text = vbNullString, 0, txt_ComFHM.Text)
                myRow(0)("pnr_fhm") = IIf(txt_PnrFHM.Text = vbNullString, 0, txt_PnrFHM.Text)
                myRow(0)("prima_tev") = IIf(txt_PrimaTEV.Text = vbNullString, 0, txt_PrimaTEV.Text)
                myRow(0)("prc_com_tev") = IIf(txt_PrcComTEV.Text = vbNullString, 0, txt_PrcComTEV.Text)
                myRow(0)("com_tev") = IIf(txt_ComTEV.Text = vbNullString, 0, txt_ComTEV.Text)
                myRow(0)("pnr_tev") = IIf(txt_PnrTEV.Text = vbNullString, 0, txt_PnrTEV.Text)
                myRow(0)("prima_rc") = IIf(txt_PrimaRC.Text = vbNullString, 0, txt_PrimaRC.Text)
                myRow(0)("prc_com_rc") = IIf(txt_PrcComRC.Text = vbNullString, 0, txt_PrcComRC.Text)
                myRow(0)("com_rc") = IIf(txt_ComRC.Text = vbNullString, 0, txt_ComRC.Text)
                myRow(0)("pnr_rc") = IIf(txt_PnrRC.Text = vbNullString, 0, txt_PnrRC.Text)
                myRow(0)("prima_casco") = IIf(txt_PrimaCSC.Text = vbNullString, 0, txt_PrimaCSC.Text)
                myRow(0)("prc_com_casco") = IIf(txt_PrcComCSC.Text = vbNullString, 0, txt_PrcComCSC.Text)
                myRow(0)("com_casco") = IIf(txt_ComCSC.Text = vbNullString, 0, txt_ComCSC.Text)
                myRow(0)("pnr_casco") = IIf(txt_PnrCSC.Text = vbNullString, 0, txt_PnrCSC.Text)
                myRow(0)("prima_guerra") = IIf(txt_PrimaGRA.Text = vbNullString, 0, txt_PrimaGRA.Text)
                myRow(0)("prc_com_guerra") = IIf(txt_PrcComGRA.Text = vbNullString, 0, txt_PrcComGRA.Text)
                myRow(0)("com_guerra") = IIf(txt_ComGRA.Text = vbNullString, 0, txt_ComGRA.Text)
                myRow(0)("pnr_guerra") = IIf(txt_PnrGRA.Text = vbNullString, 0, txt_PnrGRA.Text)
                myRow(0)("estatus") = ddl_Estatus.SelectedValue
                myRow(0)("observaciones") = txt_Observaciones.Text
            End If
        Next

        Return dtReasegurador
    End Function

    Private Function fn_ActualizaDataSubjetividad(ByVal Grupo As Integer, ByVal cod_broker As Integer, ByVal cod_cia As Integer) As DataTable
        Dim myRow() As Data.DataRow

        For Each row In gvd_Subjetividad.Rows
            Dim txt_Fecha As TextBox = TryCast(row.FindControl("txt_Fecha"), TextBox)
            Dim txt_FechaReal As TextBox = TryCast(row.FindControl("txt_FechaReal"), TextBox)
            Dim txt_Subjetividad As TextBox = TryCast(row.FindControl("txt_Subjetividad"), TextBox)
            Dim chk_Subjetividad As CheckBox = TryCast(row.FindControl("chk_Subjetividad"), CheckBox)

            myRow = dtSubjetividad.Select("cod_grupo ='" & Grupo & "' AND cod_broker = '" & cod_broker & "' AND cod_cia = '" & cod_cia & "' AND Num = '" & gvd_Subjetividad.DataKeys(row.RowIndex)("Num") & "'")

            If myRow.Length > 0 Then
                myRow(0)("Fecha") = txt_Fecha.Text
                myRow(0)("FechaReal") = txt_FechaReal.Text
                myRow(0)("Subjetividad") = txt_Subjetividad.Text
                myRow(0)("sn_Subjetividad") = IIf(chk_Subjetividad.Checked, 1, 0)
            End If
        Next

        Return dtSubjetividad
    End Function

    Private Sub gvd_Riesgo_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles gvd_Riesgo.RowDataBound
        Try

            If e.Row.RowType = DataControlRowType.DataRow Then
                Dim Tamaño() As String = Split(hid_Tamaño.Value, "|")

                TryCast(e.Row.FindControl("opt_Facultativo"), RadioButtonList).SelectedValue = sender.DataKeys(e.Row.RowIndex)("sn_facultativo")
                TryCast(e.Row.FindControl("opt_Adicional"), RadioButtonList).SelectedValue = sender.DataKeys(e.Row.RowIndex)("sn_adicional")

                TryCast(e.Row.FindControl("txt_SearchRamo"), TextBox).Width = Tamaño(0)
                TryCast(e.Row.FindControl("txt_SearchSubramo"), TextBox).Width = Tamaño(1)
                TryCast(e.Row.FindControl("txt_SearchSeccion"), TextBox).Width = Tamaño(2)
                TryCast(e.Row.FindControl("txt_SearchCobertura"), TextBox).Width = Tamaño(3)
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub gvd_RamoSbr_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvd_RamoSbr.RowCommand
        Try
            If e.CommandName = "Seccion" Then
                hid_Ramo.Value = e.CommandSource.NamingContainer.RowIndex
                hid_Seccion.Value = 0

                Dim lbl_ClaveRamo As Label = CType(gvd_RamoSbr.Rows(hid_Ramo.Value).FindControl("lbl_ClaveRamo"), Label)
                Dim lbl_ClaveSubramo As Label = CType(gvd_RamoSbr.Rows(hid_Ramo.Value).FindControl("lbl_ClaveSubramo"), Label)

                For i = 0 To gvd_RamoSbr.Rows.Count - 1
                    CType(gvd_RamoSbr.Rows(i).FindControl("btn_Detalle"), ImageButton).ImageUrl = "~/Images/blue_arrow_mini.png"
                Next
                CType(gvd_RamoSbr.Rows(hid_Ramo.Value).FindControl("btn_Detalle"), ImageButton).ImageUrl = "~/Images/red_arrow_mini.png"

                Funciones.LlenaCatGrid(gvd_Seccion, "Rie", "", " AND cod_ramo = " & lbl_ClaveRamo.Text)

                CType(gvd_Seccion.Rows(hid_Seccion.Value).FindControl("txt_SelRie"), TextBox).Text = "1"
                CType(gvd_Seccion.Rows(hid_Seccion.Value).FindControl("btn_Detalle"), ImageButton).ImageUrl = "~/Images/red_arrow_mini.png"
                CType(gvd_Seccion.Rows(hid_Seccion.Value).FindControl("div_Seccion"), HtmlGenericControl).Attributes.Add("style", "background-color: lightgreen;")
                CType(gvd_Seccion.Rows(hid_Seccion.Value).FindControl("div_Cobertura"), HtmlGenericControl).Attributes.Add("style", "display: block")

                Funciones.LlenaCatGrid(gvd_Cobertura, "Cob", "", " AND cod_ramo = " & lbl_ClaveRamo.Text & " AND cod_subramo = " & lbl_ClaveSubramo.Text)
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub gvd_Seccion_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvd_Seccion.RowCommand
        Try
            If e.CommandName = "Cobertura" Then
                hid_Seccion.Value = e.CommandSource.NamingContainer.RowIndex

                Dim lbl_ClaveRamo As Label = CType(gvd_RamoSbr.Rows(hid_Ramo.Value).FindControl("lbl_ClaveRamo"), Label)
                Dim lbl_ClaveSubramo As Label = CType(gvd_RamoSbr.Rows(hid_Ramo.Value).FindControl("lbl_ClaveSubramo"), Label)

                For i = 0 To gvd_Seccion.Rows.Count - 1
                    CType(gvd_Seccion.Rows(i).FindControl("btn_Detalle"), ImageButton).ImageUrl = "~/Images/blue_arrow_mini.png"
                Next
                CType(gvd_Seccion.Rows(hid_Seccion.Value).FindControl("btn_Detalle"), ImageButton).ImageUrl = "~/Images/red_arrow_mini.png"

                Funciones.LlenaCatGrid(gvd_Cobertura, "Cob", "", " AND cod_ramo = " & lbl_ClaveRamo.Text & " AND cod_subramo = " & lbl_ClaveSubramo.Text)
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Protected Sub AjusteTamaño(sender As Object, e As ImageClickEventArgs)
        Try
            Dim estado As Integer
            Dim pixeles As Integer
            Dim Tamaño() As String = Split(hid_Tamaño.Value, "|")
            Dim Ajuste() As String = Split(hid_Ajuste.Value, "|")

            Dim lbl_Search = TryCast(gvd_Riesgo.HeaderRow.FindControl(Replace(sender.ID, "btn", "lbl")), Label)

            If Ajuste(sender.AlternateText) = "0" Then
                estado = 1
                pixeles = 50
                lbl_Search.Width = 10
                lbl_Search.Text = ". . ."
                sender.ImageUrl = "~/Images/mas_mini.png"
            Else
                estado = 0
                pixeles = 200
                lbl_Search.Width = 60
                lbl_Search.Text = Replace(lbl_Search.ID, "lbl_Search", "")
                sender.ImageUrl = "~/Images/quitar_mini.png"
            End If

            Ajuste(sender.AlternateText) = estado
            Tamaño(sender.AlternateText) = pixeles

            hid_Tamaño.Value = Tamaño(0) & "|" & Tamaño(1) & "|" & Tamaño(2) & "|" & Tamaño(3)
            hid_Ajuste.Value = Ajuste(0) & "|" & Ajuste(1) & "|" & Ajuste(2) & "|" & Ajuste(3)

            For Each row In gvd_Riesgo.Rows
                TryCast(row.FindControl(Replace(sender.ID, "btn", "txt")), TextBox).Width = pixeles
            Next
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub SeleccionaAgrupacion(ByVal IndiceAnterior As Integer, ByVal Indice As Integer, Optional ByVal blnSeleccion As Boolean = True)
        Dim cod_cia As Integer

        If IndiceAnterior > -1 Then
            fn_ActualizaDataReparto(gvd_Agrupacion.DataKeys(IndiceAnterior)("cod_grupo"))
            fn_ActualizaDataDistribucion(gvd_Agrupacion.DataKeys(IndiceAnterior)("cod_grupo"))
            fn_ActualizaDataBroker(gvd_Agrupacion.DataKeys(IndiceAnterior)("cod_grupo"))

            If hid_IndiceBroker.Value > -1 Then
                fn_ActualizaDataReasegurador(gvd_Agrupacion.DataKeys(IndiceAnterior)("cod_grupo"), gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"))

                If gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker") > 0 Then
                    cod_cia = 0
                Else
                    If gvd_Reasegurador.Rows.Count > 0 And hid_IndiceReas.Value > -1 Then
                        cod_cia = gvd_Reasegurador.DataKeys(hid_IndiceReas.Value)("cod_cia")
                    End If
                End If

                fn_ActualizaDataSubjetividad(gvd_Agrupacion.DataKeys(IndiceAnterior)("cod_grupo"), gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"), cod_cia)
            End If
        End If

        hid_IndiceGrupo.Value = Indice
        hid_IndiceGrupoDet.Value = Indice

        If blnSeleccion = True And Indice > -1 Then
            Funciones.EjecutaFuncion("fn_CerrarModalSimple('#Subjetividad');", "CierraSubjetividad")
            Funciones.EjecutaFuncion("fn_CerrarModalSimple('#Comisiones')", "CierraComisiones")

            lbl_Intermediarios.Text = CType(gvd_Agrupacion.Rows(Indice).FindControl("lnk_Descripcion"), LinkButton).Text
            lbl_Intermediarios.ToolTip = CType(gvd_Agrupacion.Rows(Indice).FindControl("lnk_Descripcion"), LinkButton).Text

            lbl_Distribucion.Text = CType(gvd_Agrupacion.Rows(Indice).FindControl("lnk_Descripcion"), LinkButton).Text
            lbl_Distribucion.ToolTip = CType(gvd_Agrupacion.Rows(Indice).FindControl("lnk_Descripcion"), LinkButton).Text

            For Each row In gvd_Agrupacion.Rows
                TryCast(row.FindControl("lnk_Descripcion"), LinkButton).Font.Bold = False
                TryCast(row.FindControl("lnk_Descripcion"), LinkButton).ForeColor = Drawing.ColorTranslator.FromHtml("#555")
                TryCast(row.FindControl("lnk_Descripcion"), LinkButton).BorderColor = Drawing.ColorTranslator.FromHtml("#ccc")

                TryCast(row.FindControl("txt_LimResp"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#555")
                TryCast(row.FindControl("txt_PrimaNeta"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#555")
                TryCast(row.FindControl("txt_PrimaTEV"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#555")
                TryCast(row.FindControl("txt_PrimaFHM"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#555")
            Next

            CType(gvd_Agrupacion.Rows(Indice).FindControl("lnk_Descripcion"), LinkButton).Font.Bold = True
            CType(gvd_Agrupacion.Rows(Indice).FindControl("lnk_Descripcion"), LinkButton).ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")
            CType(gvd_Agrupacion.Rows(Indice).FindControl("lnk_Descripcion"), LinkButton).BorderColor = Drawing.ColorTranslator.FromHtml("#003A5D")

            CType(gvd_Agrupacion.Rows(Indice).FindControl("txt_LimResp"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")
            CType(gvd_Agrupacion.Rows(Indice).FindControl("txt_PrimaNeta"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")
            CType(gvd_Agrupacion.Rows(Indice).FindControl("txt_PrimaTEV"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")
            CType(gvd_Agrupacion.Rows(Indice).FindControl("txt_PrimaFHM"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")

            'Llena Información de Reparto
            LlenaGridReparto(False, gvd_Agrupacion.DataKeys(Indice)("cod_grupo"))

            'Llena Información de Distribución
            LlenaGridDistribucion(False, gvd_Agrupacion.DataKeys(Indice)("cod_grupo"))

            'Llena Información de Broker
            LlenaGridBroker(False, gvd_Agrupacion.DataKeys(Indice)("cod_grupo"))
            hid_IndiceBroker.Value = IIf(gvd_Intermediario.Rows.Count > 0, 0, -1)

            LlenaGridReasegurador(False, gvd_Agrupacion.DataKeys(Indice)("cod_grupo"), 0)

            LlenaGridSubjetividad(False, gvd_Agrupacion.DataKeys(Indice)("cod_grupo"), 0, 0)

            btn_AddCia.Attributes.Add("disabled", "true")

            SeleccionaBroker(-1, hid_IndiceBroker.Value)
            EstadoColumnasAgrupacion()
            ValidaRamosGrupo(CType(gvd_Agrupacion.Rows(Indice).FindControl("txt_Ramos"), TextBox).Text)
            EjecutaCalculos()
        End If
    End Sub

    Private Sub EstadoColumnasAgrupacion()
        gvd_Agrupacion.Columns(5).Visible = False
        gvd_Agrupacion.Columns(6).Visible = False
        gvd_Agrupacion.Columns(7).Visible = False
        gvd_Agrupacion.Columns(8).Visible = False
        gvd_Agrupacion.Columns(9).Visible = False
        gvd_Agrupacion.Columns(10).Visible = False

        For Each row In gvd_Agrupacion.Rows
            Dim Ramos() As String = Split(TryCast(row.FindControl("txt_Ramos"), TextBox).Text, ",")
            If Ramos.Contains(Ramo.Incendio) Then gvd_Agrupacion.Columns(5).Visible = True
            If Ramos.Contains(Ramo.Hidro) Then gvd_Agrupacion.Columns(6).Visible = True
            If Ramos.Contains(Ramo.Terremoto) Then gvd_Agrupacion.Columns(7).Visible = True
            If Ramos.Contains(Ramo.RCAviones) Then gvd_Agrupacion.Columns(8).Visible = True
            If Ramos.Contains(Ramo.CascosAereos) Then gvd_Agrupacion.Columns(9).Visible = True
            If Ramos.Contains(Ramo.CascosAereos) Then gvd_Agrupacion.Columns(10).Visible = True
        Next
    End Sub
    Private Sub ObtieneDatosComision(ByRef GridControl As GridView, ByVal Indice As Integer, ByVal Tipo() As String)
        Dim Prc As String
        Dim Com As String
        Dim Pnr As String

        'Corregir las cadenas NAN con Isnumeric
        For Each item In Tipo
            Prc = IIf(Not IsNumeric(CType(GridControl.Rows(Indice).FindControl("txt_PrcCom" & item), TextBox).Text), 0, CType(GridControl.Rows(Indice).FindControl("txt_PrcCom" & item), TextBox).Text)
            Com = IIf(Not IsNumeric(CType(GridControl.Rows(Indice).FindControl("txt_Com" & item), TextBox).Text), 0, CType(GridControl.Rows(Indice).FindControl("txt_Com" & item), TextBox).Text)
            Pnr = IIf(Not IsNumeric(CType(GridControl.Rows(Indice).FindControl("txt_Pnr" & item), TextBox).Text), 0, CType(GridControl.Rows(Indice).FindControl("txt_Pnr" & item), TextBox).Text)

            TryCast(Master.Contenedor.FindControl("txt_PrcCom" & item), TextBox).Text = String.Format("{0:#0.0000}", CDbl(Prc))
            TryCast(Master.Contenedor.FindControl("txt_Com" & item), TextBox).Text = String.Format("{0:#,#0.00}", CDbl(Com))
            TryCast(Master.Contenedor.FindControl("txt_Com" & item & "Aux"), TextBox).Text = CType(GridControl.Rows(Indice).FindControl("txt_Com" & item), TextBox).Text
            TryCast(Master.Contenedor.FindControl("txt_Pnr" & item), TextBox).Text = String.Format("{0:#,#0.00}", CDbl(Pnr))
        Next

    End Sub

    Private Sub SeleccionaBroker(ByVal IndiceAnterior As Integer, ByVal Indice As Integer)
        Dim cod_cia As Integer

        lbl_Reaseguradores.Text = vbNullString
        lbl_Reaseguradores.ToolTip = vbNullString

        Funciones.EjecutaFuncion("fn_CerrarModalSimple('#Subjetividad');", "CierraSubjetividad")
        Funciones.EjecutaFuncion("fn_CerrarModalSimple('#Comisiones')", "CierraComisiones")

        If Indice > -1 Then
            btn_AddCia.Attributes.Remove("disabled")

            If IndiceAnterior > -1 Then
                fn_ActualizaDataReasegurador(gvd_Intermediario.DataKeys(IndiceAnterior)("cod_grupo"), gvd_Intermediario.DataKeys(IndiceAnterior)("cod_broker"))

                If gvd_Intermediario.DataKeys(IndiceAnterior)("cod_broker") > 0 Then
                    cod_cia = 0
                Else
                    If gvd_Reasegurador.Rows.Count > 0 And hid_IndiceReas.Value > -1 Then
                        cod_cia = gvd_Reasegurador.DataKeys(hid_IndiceReas.Value)("cod_cia")
                    End If
                End If

                fn_ActualizaDataSubjetividad(gvd_Intermediario.DataKeys(IndiceAnterior)("cod_grupo"), gvd_Intermediario.DataKeys(IndiceAnterior)("cod_broker"), cod_cia)
            End If

            hid_IndiceBroker.Value = Indice

            For Each row In gvd_Intermediario.Rows
                TryCast(row.FindControl("lnk_Clave"), LinkButton).Font.Bold = False
                TryCast(row.FindControl("lnk_Clave"), LinkButton).ForeColor = Drawing.ColorTranslator.FromHtml("#555")
                TryCast(row.FindControl("lnk_Clave"), LinkButton).BorderColor = Drawing.ColorTranslator.FromHtml("#ccc")

                TryCast(row.FindControl("txt_Descripcion"), TextBox).Font.Bold = False
                TryCast(row.FindControl("txt_Descripcion"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#555")
                TryCast(row.FindControl("txt_Descripcion"), TextBox).BorderColor = Drawing.ColorTranslator.FromHtml("#ccc")
            Next

            CType(gvd_Intermediario.Rows(Indice).FindControl("lnk_Clave"), LinkButton).Font.Bold = True
            CType(gvd_Intermediario.Rows(Indice).FindControl("lnk_Clave"), LinkButton).ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")
            CType(gvd_Intermediario.Rows(Indice).FindControl("lnk_Clave"), LinkButton).BorderColor = Drawing.ColorTranslator.FromHtml("#003A5D")

            CType(gvd_Intermediario.Rows(Indice).FindControl("txt_Descripcion"), TextBox).Font.Bold = True
            CType(gvd_Intermediario.Rows(Indice).FindControl("txt_Descripcion"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")
            CType(gvd_Intermediario.Rows(Indice).FindControl("txt_Descripcion"), TextBox).BorderColor = Drawing.ColorTranslator.FromHtml("#003A5D")

            lbl_Reaseguradores.Text = CType(gvd_Intermediario.Rows(Indice).FindControl("txt_Descripcion"), TextBox).Text
            lbl_Reaseguradores.ToolTip = CType(gvd_Intermediario.Rows(Indice).FindControl("txt_Descripcion"), TextBox).Text

            'Llena Información de Reaseguradores
            LlenaGridReasegurador(False, gvd_Intermediario.DataKeys(Indice)("cod_grupo"), gvd_Intermediario.DataKeys(Indice)("cod_broker"))

            ' Si es mediante Intermediario
            If gvd_Intermediario.DataKeys(Indice)("cod_broker") > 0 Then
                LlenaGridSubjetividad(False, gvd_Intermediario.DataKeys(Indice)("cod_grupo"),
                                      gvd_Intermediario.DataKeys(Indice)("cod_broker"), 0)
                txt_Observaciones.Text = CType(gvd_Intermediario.Rows(Indice).FindControl("txt_Observaciones"), TextBox).Text

                ObtieneDatosComision(gvd_Intermediario, Indice, {"Neta", "INC", "FHM", "TEV", "RC", "CSC", "GRA"})

                lbl_Comisiones.Text = Mid(CType(gvd_Intermediario.Rows(Indice).FindControl("txt_Descripcion"), TextBox).Text, 1, 20) & "..."
                lbl_Comisiones.ToolTip = CType(gvd_Intermediario.Rows(Indice).FindControl("txt_Descripcion"), TextBox).Text

                lbl_Subjetividades.Text = Mid(CType(gvd_Intermediario.Rows(Indice).FindControl("txt_Descripcion"), TextBox).Text, 1, 50) & "..."
                lbl_Subjetividades.ToolTip = CType(gvd_Intermediario.Rows(Indice).FindControl("txt_Descripcion"), TextBox).Text

                If hid_Pestaña.Value = 1 Then
                    Funciones.EjecutaFuncion("fn_AbrirModalSimple('#Subjetividad')", "Subjetividad")
                    Funciones.EjecutaFuncion("fn_AbrirModalSimple('#Comisiones')", "Comisiones")
                End If
            Else 'Si es Negocio Directo
                If gvd_Reasegurador.Rows.Count > 0 Then
                    hid_IndiceReas.Value = 0
                    SeleccionaReasegurador(-1, hid_IndiceReas.Value)
                Else
                    LlenaGridSubjetividad(False, 0, 0, 0)
                    txt_Observaciones.Text = vbNullString

                    txt_PrcComNeta.Text = "0.0000"
                    txt_ComNeta.Text = "0.00"
                    txt_ComNetaAux.Text = "0"
                    txt_PnrNeta.Text = "0.00"

                    txt_PrcComINC.Text = "0.0000"
                    txt_ComINC.Text = "0.00"
                    txt_ComINCAux.Text = "0"
                    txt_PnrINC.Text = "0.00"

                    txt_PrcComTEV.Text = "0.0000"
                    txt_ComTEV.Text = "0.00"
                    txt_ComTEVAux.Text = "0"
                    txt_PnrTEV.Text = "0.00"


                    txt_PrcComFHM.Text = "0.0000"
                    txt_ComFHM.Text = "0.00"
                    txt_ComFHMAux.Text = "0"
                    txt_PnrFHM.Text = "0.00"
                End If
            End If

            'Actualiza Intermediario
            Funciones.EjecutaFuncion("fn_CalculaIntermediario(-1, " & Indice & ", ['SumaAsegurada', 'PrimaNeta', 'PrimaINC', 'PrimaTEV', 'PrimaFHM'], ['txt_LimResp', 'txt_PrimaNeta', 'txt_PrimaINC', 'txt_PrimaTEV', 'txt_PrimaFHM']);", "Intermediario")

            'Actualiza Reasegurador
            Funciones.EjecutaFuncion("fn_CalculaReasegurador(-1, -1, ['SumaAsegurada', 'PrimaNeta', 'PrimaINC', 'PrimaTEV', 'PrimaFHM'], ['txt_LimResp', 'txt_PrimaNeta', 'txt_PrimaINC', 'txt_PrimaTEV', 'txt_PrimaFHM']);", "Reasegurador")
        End If

    End Sub

    Private Sub SeleccionaReasegurador(ByVal IndiceAnterior As Integer, ByVal Indice As Integer)

        Funciones.EjecutaFuncion("fn_CerrarModalSimple('#Subjetividad');", "CierraSubjetividad")
        Funciones.EjecutaFuncion("fn_CerrarModalSimple('#Comisiones')", "CierraComisiones")

        If Indice > -1 Then

            If IndiceAnterior > -1 Then
                'Solo si es Negocio Directo
                If gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker") = 0 Then
                    fn_ActualizaDataSubjetividad(gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_grupo"),
                                                 gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"),
                                                 gvd_Reasegurador.DataKeys(IndiceAnterior)("cod_cia"))
                End If
            End If

            hid_IndiceReas.Value = Indice

            For Each row In gvd_Reasegurador.Rows
                TryCast(row.FindControl("lnk_Clave"), LinkButton).Font.Bold = False
                TryCast(row.FindControl("lnk_Clave"), LinkButton).ForeColor = Drawing.ColorTranslator.FromHtml("#555")
                TryCast(row.FindControl("lnk_Clave"), LinkButton).BorderColor = Drawing.ColorTranslator.FromHtml("#ccc")

                TryCast(row.FindControl("txt_Descripcion"), TextBox).Font.Bold = False
                TryCast(row.FindControl("txt_Descripcion"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#555")
                TryCast(row.FindControl("txt_Descripcion"), TextBox).BorderColor = Drawing.ColorTranslator.FromHtml("#ccc")
            Next

            CType(gvd_Reasegurador.Rows(Indice).FindControl("lnk_Clave"), LinkButton).Font.Bold = True
            CType(gvd_Reasegurador.Rows(Indice).FindControl("lnk_Clave"), LinkButton).ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")
            CType(gvd_Reasegurador.Rows(Indice).FindControl("lnk_Clave"), LinkButton).BorderColor = Drawing.ColorTranslator.FromHtml("#003A5D")

            CType(gvd_Reasegurador.Rows(Indice).FindControl("txt_Descripcion"), TextBox).Font.Bold = True
            CType(gvd_Reasegurador.Rows(Indice).FindControl("txt_Descripcion"), TextBox).ForeColor = Drawing.ColorTranslator.FromHtml("#003A5D")
            CType(gvd_Reasegurador.Rows(Indice).FindControl("txt_Descripcion"), TextBox).BorderColor = Drawing.ColorTranslator.FromHtml("#003A5D")

            If hid_Pestaña.Value = 1 Then
                Funciones.EjecutaFuncion("fn_AbrirModalSimple('#Subjetividad')", "Subjetividad")
                Funciones.EjecutaFuncion("fn_AbrirModalSimple('#Comisiones')", "Comisiones")
            End If

            If gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker") = 0 Then
                LlenaGridSubjetividad(False,
                                      gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_grupo"),
                                      gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"),
                                      gvd_Reasegurador.DataKeys(Indice)("cod_cia"))

                txt_Observaciones.Text = CType(gvd_Reasegurador.Rows(hid_IndiceReas.Value).FindControl("txt_Observaciones"), TextBox).Text

                ObtieneDatosComision(gvd_Reasegurador, Indice, {"Neta", "INC", "FHM", "TEV", "RC", "CSC", "GRA"})

                lbl_Comisiones.Text = Mid(CType(gvd_Reasegurador.Rows(Indice).FindControl("txt_Descripcion"), TextBox).Text, 1, 20) & "..."
                lbl_Comisiones.ToolTip = CType(gvd_Reasegurador.Rows(Indice).FindControl("txt_Descripcion"), TextBox).Text

                lbl_Subjetividades.Text = Mid(CType(gvd_Reasegurador.Rows(Indice).FindControl("txt_Descripcion"), TextBox).Text, 1, 50) & "..."
                lbl_Subjetividades.ToolTip = CType(gvd_Reasegurador.Rows(Indice).FindControl("txt_Descripcion"), TextBox).Text
            End If

            'Actualiza Reasegurador
            Funciones.EjecutaFuncion("fn_CalculaReasegurador(-1, " & Indice & ", ['SumaAsegurada', 'PrimaNeta', 'PrimaINC', 'PrimaTEV', 'PrimaFHM'], ['txt_LimResp', 'txt_PrimaNeta', 'txt_PrimaINC', 'txt_PrimaTEV', 'txt_PrimaFHM']);", "Reasegurador")
        End If
    End Sub

    Private Sub gvd_Agrupacion_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvd_Agrupacion.RowCommand
        Try

            If e.CommandName = "Descripcion" Then
                SeleccionaAgrupacion(hid_IndiceGrupo.Value, e.CommandSource.NamingContainer.RowIndex)
            ElseIf e.CommandName = "Incisos" Then
                btn_AddRiesgoGrupo.Visible = False
                btn_RemoveRiesgoGrupo.Visible = True

                hid_IndiceGrupoDet.Value = e.CommandSource.NamingContainer.RowIndex

                Dim txt_Incisos As TextBox = CType(gvd_Agrupacion.Rows(hid_IndiceGrupoDet.Value).FindControl("txt_Incisos"), TextBox)

                If Len(txt_Incisos.Text) > 0 Then
                    Dim incisos() As String = Split(txt_Incisos.Text, ",")

                    LlenaGridIncisos(True)

                    For Each row In gvd_Riesgo.Rows
                        If incisos.Contains(TryCast(row.FindControl("lbl_Inciso"), Label).Text) Then
                            dtIncisos.Rows.Add(TryCast(row.FindControl("lbl_Inciso"), Label).Text,
                                               TryCast(row.FindControl("txt_ClaveRamo"), TextBox).Text,
                                               TryCast(row.FindControl("txt_SearchRamo"), TextBox).Text,
                                               TryCast(row.FindControl("txt_SearchSubramo"), TextBox).Text,
                                               TryCast(row.FindControl("txt_SearchSeccion"), TextBox).Text,
                                               TryCast(row.FindControl("txt_SearchCobertura"), TextBox).Text,
                                               TryCast(row.FindControl("opt_Adicional"), RadioButtonList).SelectedValue,
                                               IIf(TryCast(row.FindControl("txt_ValoresTotalesAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_ValoresTotalesAux"), TextBox).Text),
                                               IIf(TryCast(row.FindControl("txt_LimRespAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_LimRespAux"), TextBox).Text),
                                               IIf(TryCast(row.FindControl("txt_PrimaNetaAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaNetaAux"), TextBox).Text),
                                               IIf(TryCast(row.FindControl("txt_PrimaINCAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaINCAux"), TextBox).Text),
                                               IIf(TryCast(row.FindControl("txt_PrimaFHMAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaFHMAux"), TextBox).Text),
                                               IIf(TryCast(row.FindControl("txt_PrimaTEVAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaTEVAux"), TextBox).Text),
                                               IIf(TryCast(row.FindControl("txt_PrimaRCAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaRCAux"), TextBox).Text),
                                               IIf(TryCast(row.FindControl("txt_PrimaCSCAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaCSCAux"), TextBox).Text),
                                               IIf(TryCast(row.FindControl("txt_PrimaGRAAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaGRAAux"), TextBox).Text))
                        End If
                    Next

                    LlenaGridIncisos(False)

                    Funciones.AbrirModal("#Incisos")
                Else
                    Mensaje.MuestraMensaje("Mesa de Control", "La Agrupación no contiene Riesgos", TipoMsg.Advertencia)
                End If

            ElseIf e.CommandName = "Agregar" Then

                btn_AddRiesgoGrupo.Visible = True
                btn_RemoveRiesgoGrupo.Visible = False

                hid_IndiceGrupoDet.Value = e.CommandSource.NamingContainer.RowIndex
                Dim incisos() As String = Split(CType(gvd_Agrupacion.Rows(hid_IndiceGrupoDet.Value).FindControl("txt_Incisos"), TextBox).Text, ",")

                LlenaGridIncisos(True)

                For Each row In gvd_Riesgo.Rows
                    If Not incisos.Contains(TryCast(row.FindControl("lbl_Inciso"), Label).Text) And
                        TryCast(row.FindControl("lbl_Inciso"), Label).Text <> "" And
                        TryCast(row.FindControl("opt_Facultativo"), RadioButtonList).SelectedValue = 1 Then
                        dtIncisos.Rows.Add(TryCast(row.FindControl("lbl_Inciso"), Label).Text,
                                           TryCast(row.FindControl("txt_ClaveRamo"), TextBox).Text,
                                           TryCast(row.FindControl("txt_SearchRamo"), TextBox).Text,
                                           TryCast(row.FindControl("txt_SearchSubramo"), TextBox).Text,
                                           TryCast(row.FindControl("txt_SearchSeccion"), TextBox).Text,
                                           TryCast(row.FindControl("txt_SearchCobertura"), TextBox).Text,
                                           TryCast(row.FindControl("opt_Adicional"), RadioButtonList).SelectedValue,
                                           IIf(TryCast(row.FindControl("txt_ValoresTotalesAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_ValoresTotalesAux"), TextBox).Text),
                                           IIf(TryCast(row.FindControl("txt_LimRespAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_LimRespAux"), TextBox).Text),
                                           IIf(TryCast(row.FindControl("txt_PrimaNetaAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaNetaAux"), TextBox).Text),
                                           IIf(TryCast(row.FindControl("txt_PrimaINCAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaINCAux"), TextBox).Text),
                                           IIf(TryCast(row.FindControl("txt_PrimaFHMAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaFHMAux"), TextBox).Text),
                                           IIf(TryCast(row.FindControl("txt_PrimaTEVAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaTEVAux"), TextBox).Text),
                                           IIf(TryCast(row.FindControl("txt_PrimaRCAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaRCAux"), TextBox).Text),
                                           IIf(TryCast(row.FindControl("txt_PrimaCSCAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaCSCAux"), TextBox).Text),
                                           IIf(TryCast(row.FindControl("txt_PrimaGRAAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaGRAAux"), TextBox).Text))
                    End If
                Next

                LlenaGridIncisos(False)

                If gvd_RiesgosGrupo.Rows.Count > 0 Then
                    Funciones.AbrirModal("#Incisos")
                Else
                    Mensaje.MuestraMensaje("Mesa de Control", "No existen mas Riesgos disponibles para agrupar", TipoMsg.Advertencia)
                End If
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_RemoveRiesgoGrupo_Click(sender As Object, e As EventArgs) Handles btn_RemoveRiesgoGrupo.Click
        Try
            Dim myRow() As Data.DataRow

            Dim strRiesgos As String = ObtieneSeleccionados(gvd_RiesgosGrupo, "chk_Sel", "cod_inciso")

            If Len(strRiesgos) > 0 Then
                Dim Incisos As String = ObtieneSeleccionados(gvd_RiesgosGrupo, "chk_Sel", "cod_inciso", False)
                Dim strRamos As String = ObtieneSeleccionados(gvd_RiesgosGrupo, "chk_Sel", "cod_ramo", False)

                myRow = dtIncisos.Select("cod_inciso IN (" & strRiesgos & ")")

                For Each item In myRow
                    item.Delete()
                Next
                dtIncisos.AcceptChanges()

                LiberaRiesgos(strRiesgos)

                CType(gvd_Agrupacion.Rows(hid_IndiceGrupoDet.Value).FindControl("txt_Incisos"), TextBox).Text = Incisos
                CType(gvd_Agrupacion.Rows(hid_IndiceGrupoDet.Value).FindControl("txt_Ramos"), TextBox).Text = strRamos
                myRow = dtAgrupacion.Select("cod_grupo ='" & gvd_Agrupacion.DataKeys(hid_IndiceGrupoDet.Value)("cod_grupo") & "'")
                If myRow.Length > 0 Then
                    myRow(0)("incisos") = Incisos
                    myRow(0)("ramos") = strRamos
                End If

                EstadoColumnasAgrupacion()

                'Actualiza Agrupacion
                Funciones.EjecutaFuncion("fn_ActualizaGrupo(" & gvd_Agrupacion.DataKeys(hid_IndiceGrupoDet.Value)("cod_grupo") & ", 0, ['SumaAsegurada','PrimaNeta','PrimaINC','PrimaTEV','PrimaFHM'], ['txt_LimRespAux','txt_PrimaNetaAux','txt_PrimaINCAux','txt_PrimaTEVAux','txt_PrimaFHMAux'], 1)", "Grupos")

                'Si se trata del Grupo en Pantalla
                If hid_IndiceGrupo.Value = hid_IndiceGrupoDet.Value Then
                    EjecutaCalculos()
                    ValidaRamosGrupo(CType(gvd_Agrupacion.Rows(hid_IndiceGrupoDet.Value).FindControl("txt_Ramos"), TextBox).Text)
                End If

                LlenaGridIncisos(False)

                If gvd_RiesgosGrupo.Rows.Count = 0 Then
                    Funciones.EjecutaFuncion("fn_CerrarModal('#Incisos');", "CierraRiesgos")
                    Mensaje.MuestraMensaje("Mesa de Control", "La Agrupación ya no contiene ningún Riesgo", TipoMsg.Advertencia)
                End If
            Else
                Mensaje.MuestraMensaje("Mesa de Control", "No se seleccionó ningun elemento", TipoMsg.Advertencia)
            End If

            Funciones.EjecutaFuncion("fn_CerrarModal('#EsperaModal');", "CerrarModal")

        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_AddRiesgoGrupo_Click(sender As Object, e As EventArgs) Handles btn_AddRiesgoGrupo.Click
        Try
            Dim myRow() As Data.DataRow
            Dim strRiesgos As String = ObtieneSeleccionados(gvd_RiesgosGrupo, "chk_Sel", "cod_inciso")


            If Len(strRiesgos) > 0 Then
                Dim strRamos As String = ObtieneSeleccionados(gvd_RiesgosGrupo, "chk_Sel", "cod_ramo")

                myRow = dtIncisos.Select("cod_inciso IN (" & strRiesgos & ")")

                For Each item In myRow
                    item.Delete()
                Next
                dtIncisos.AcceptChanges()

                Dim Riesgos() As String = Split(strRiesgos, ",")
                For Each row In gvd_Riesgo.Rows
                    If Riesgos.Contains(TryCast(row.FindControl("lbl_Inciso"), Label).Text) Then
                        TryCast(row.FindControl("chk_Riesgo"), CheckBox).Enabled = False
                        TryCast(row.FindControl("chk_Riesgo"), CheckBox).Checked = True
                        TryCast(row.FindControl("opt_Facultativo"), RadioButtonList).Enabled = False
                    End If
                Next

                If Len(CType(gvd_Agrupacion.Rows(hid_IndiceGrupoDet.Value).FindControl("txt_Incisos"), TextBox).Text) > 0 Then
                    strRiesgos = "," & strRiesgos
                End If

                If Len(CType(gvd_Agrupacion.Rows(hid_IndiceGrupoDet.Value).FindControl("txt_Ramos"), TextBox).Text) > 0 Then
                    strRamos = "," & strRamos
                End If

                CType(gvd_Agrupacion.Rows(hid_IndiceGrupoDet.Value).FindControl("txt_Incisos"), TextBox).Text = CType(gvd_Agrupacion.Rows(hid_IndiceGrupoDet.Value).FindControl("txt_Incisos"), TextBox).Text & strRiesgos
                CType(gvd_Agrupacion.Rows(hid_IndiceGrupoDet.Value).FindControl("txt_Ramos"), TextBox).Text = CType(gvd_Agrupacion.Rows(hid_IndiceGrupoDet.Value).FindControl("txt_Ramos"), TextBox).Text & strRamos

                myRow = dtAgrupacion.Select("cod_grupo ='" & gvd_Agrupacion.DataKeys(hid_IndiceGrupoDet.Value)("cod_grupo") & "'")
                If myRow.Length > 0 Then
                    myRow(0)("incisos") = CType(gvd_Agrupacion.Rows(hid_IndiceGrupoDet.Value).FindControl("txt_Incisos"), TextBox).Text
                    myRow(0)("ramos") = CType(gvd_Agrupacion.Rows(hid_IndiceGrupoDet.Value).FindControl("txt_Ramos"), TextBox).Text
                End If

                EstadoColumnasAgrupacion()

                'Actualiza Agrupacion
                Funciones.EjecutaFuncion("fn_ActualizaGrupo(" & gvd_Agrupacion.DataKeys(hid_IndiceGrupoDet.Value)("cod_grupo") & ", 0, ['SumaAsegurada','PrimaNeta','PrimaINC','PrimaFHM','PrimaTEV','PrimaRC','PrimaCSC','PrimaGRA'], ['txt_LimRespAux','txt_PrimaNetaAux','txt_PrimaINCAux','txt_PrimaFHMAux','txt_PrimaTEVAux','txt_PrimaRCAux','txt_PrimaCSCAux','txt_PrimaGRAAux'], 1)", "Grupos")

                If hid_IndiceGrupo.Value = hid_IndiceGrupoDet.Value Then
                    EjecutaCalculos()
                    ValidaRamosGrupo(CType(gvd_Agrupacion.Rows(hid_IndiceGrupoDet.Value).FindControl("txt_Ramos"), TextBox).Text)
                End If

                LlenaGridIncisos(False)

                If gvd_RiesgosGrupo.Rows.Count = 0 Then
                    Funciones.EjecutaFuncion("fn_CerrarModal('#Incisos');", "CierraRiesgos")
                    Mensaje.MuestraMensaje("Mesa de Control", "No existen mas Riesgos disponibles para agrupar", TipoMsg.Advertencia)
                End If

            Else
                Mensaje.MuestraMensaje("Mesa de Control", "No se seleccionó ningun elemento", TipoMsg.Advertencia)
            End If

            Funciones.EjecutaFuncion("fn_CerrarModal('#EsperaModal');", "CerrarModal")
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub EjecutaCalculos()

        'Actualiza Reparto General
        Funciones.EjecutaFuncion("fn_CalculaReparto(-1, -1, ['SumaAsegurada', 'PrimaNeta', 'PrimaINC', 'PrimaTEV', 'PrimaFHM', 'PrimaRC', 'PrimaCSC', 'PrimaGRA'], ['txt_LimResp', 'txt_PrimaNeta', 'txt_PrimaINC', 'txt_PrimaTEV', 'txt_PrimaFHM', 'txt_PrimaRC' , 'txt_PrimaCSC', 'txt_PrimaGRA']);", "Reparto")

        'Actualiza Distribución para GMX
        Funciones.EjecutaFuncion("fn_CalculaDistribucionGMX(-1, -1, ['SumaAsegurada', 'PrimaNeta', 'PrimaINC', 'PrimaTEV', 'PrimaFHM', 'PrimaRC', 'PrimaCSC', 'PrimaGRA'], ['txt_LimResp', 'txt_PrimaNeta', 'txt_PrimaINC', 'txt_PrimaTEV', 'txt_PrimaFHM', 'txt_PrimaRC' , 'txt_PrimaCSC', 'txt_PrimaGRA']);", "Distribucion")

        'Actualiza Intermediario
        Funciones.EjecutaFuncion("fn_CalculaIntermediario(-1, -1, ['SumaAsegurada', 'PrimaNeta', 'PrimaINC', 'PrimaTEV', 'PrimaFHM', 'PrimaRC', 'PrimaCSC', 'PrimaGRA'], ['txt_LimResp', 'txt_PrimaNeta', 'txt_PrimaINC', 'txt_PrimaTEV', 'txt_PrimaFHM', 'txt_PrimaRC' , 'txt_PrimaCSC', 'txt_PrimaGRA']);", "Intermediario")

        'Actualiza Reasegurador
        Funciones.EjecutaFuncion("fn_CalculaReasegurador(-1, -1, ['SumaAsegurada', 'PrimaNeta', 'PrimaINC', 'PrimaTEV', 'PrimaFHM', 'PrimaRC', 'PrimaCSC', 'PrimaGRA'], ['txt_LimResp', 'txt_PrimaNeta', 'txt_PrimaINC', 'txt_PrimaTEV', 'txt_PrimaFHM', 'txt_PrimaRC' , 'txt_PrimaCSC', 'txt_PrimaGRA']);", "Reasegurador")

    End Sub

    Private Sub Agrega_Intermediario()
        Try

            'Si existe agrupación activa
            If hid_IndiceGrupo.Value > -1 Then
                fn_ActualizaDataBroker(gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"))
                Dim Datos() As String
                Dim Seleccionados As String = Master.mSeleccionados

                If Len(Seleccionados) > 0 Then
                    Datos = Split(Seleccionados.Substring(0, Seleccionados.Length - 1), "|")

                    For Each dato In Datos
                        dtBroker.Rows.Add(gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"), Split(dato, "~")(0), Split(dato, "~")(1),
                                          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "")
                    Next

                    LlenaGridBroker(False, gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"))
                    SeleccionaBroker(hid_IndiceBroker.Value, gvd_Intermediario.Rows.Count - 1)
                End If
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub Agrega_Reasegurador()
        Try
            'Si existe agrupación activa
            If hid_IndiceBroker.Value > -1 Then
                fn_ActualizaDataReasegurador(gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_grupo"), gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"))
                Dim Datos() As String
                Dim Seleccionados As String = Master.mSeleccionados

                If Len(Seleccionados) > 0 Then
                    Datos = Split(Seleccionados.Substring(0, Seleccionados.Length - 1), "|")

                    For Each dato In Datos
                        dtReasegurador.Rows.Add(gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_grupo"), gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"),
                                                Split(dato, "~")(0), Split(dato, "~")(1), Split(dato, "~")(2), Split(dato, "~")(3), Split(dato, "~")(4),
                                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "")
                    Next

                    LlenaGridReasegurador(False, gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_grupo"), gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"))

                    SeleccionaReasegurador(hid_IndiceReas.Value, gvd_Reasegurador.Rows.Count - 1)
                End If
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub Agrega_Coasegurador()

        'Si existe agrupación activa
        If hid_IndiceGrupo.Value > -1 Then
            fn_ActualizaDataReparto(gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"))
            Dim Datos() As String
            Dim Seleccionados As String = Master.mSeleccionados

            If Len(Seleccionados) > 0 Then
                Datos = Split(Seleccionados.Substring(0, Seleccionados.Length - 1), "|")

                For Each dato In Datos
                    dtReparto.Rows.Add(gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"),
                                       Split(dato, "~")(0), Split(dato, "~")(1), "0.0000", 0,
                                       0, 0, 0, 0, 0, 0, 0, 0)
                Next

                LlenaGridReparto(False, gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"))
            End If
        End If
    End Sub

    Private Sub Agrega_Subjetividad()
        Try
            Dim Num As Integer = 1
            Dim cod_cia As Integer = 0

            'Si existe agrupación activa
            If hid_IndiceBroker.Value > -1 Or hid_IndiceReas.Value > -1 Then

                If gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker") = 0 Then
                    cod_cia = gvd_Reasegurador.DataKeys(hid_IndiceReas.Value)("cod_cia")
                End If
                fn_ActualizaDataSubjetividad(gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_grupo"),
                                             gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"),
                                             cod_cia)


                If gvd_Subjetividad.Rows.Count > 0 Then
                    Num = gvd_Subjetividad.DataKeys(gvd_Subjetividad.Rows.Count - 1)("Num") + 1
                End If

                dtSubjetividad.Rows.Add(gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_grupo"),
                                        gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"),
                                        cod_cia,
                                        Num,
                                        "", "", "", 0)

                LlenaGridSubjetividad(False, gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_grupo"),
                                      gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"),
                                      cod_cia)

            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub Agrega_Capa()
        Dim Capa As Integer = 1

        'Si existe agrupación activa
        If hid_IndiceGrupo.Value > -1 Then
            fn_ActualizaDataCapa(gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"))

            If gvd_Capas.Rows.Count > 0 Then
                Capa = gvd_Capas.DataKeys(gvd_Capas.Rows.Count - 1)("cod_capa") + 1
            End If

            dtCapas.Rows.Add(gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"),
                             Capa, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)

            LlenaGridCapas(False, gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"))
        End If
    End Sub

    Private Sub gvd_Intermediario_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvd_Intermediario.RowCommand
        Try
            If e.CommandName = "Reasegurador" Then
                SeleccionaBroker(hid_IndiceBroker.Value, e.CommandSource.NamingContainer.RowIndex)
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_AddSubjetividad_Click(sender As Object, e As EventArgs) Handles btn_AddSubjetividad.Click
        Try
            Agrega_Subjetividad()
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub gvd_Reasegurador_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvd_Reasegurador.RowCommand
        Try
            If e.CommandName = "Subjetividad" Then
                SeleccionaReasegurador(hid_IndiceReas.Value, e.CommandSource.NamingContainer.RowIndex)
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_RemoveBroker_Click(sender As Object, e As EventArgs) Handles btn_RemoveBroker.Click
        Try
            Dim strSel As String = ObtieneSeleccionados(gvd_Intermediario, "chk_Sel", "cod_broker")

            If Len(strSel) > 0 Then
                Quita_Intermediario(gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"), strSel)

                LlenaGridBroker(False, gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"))

                hid_IndiceBroker.Value = IIf(gvd_Intermediario.Rows.Count > 0, 0, -1)

                'Llena Información de Broker
                LlenaGridReasegurador(False, 0)

                'Llena Información de Broker
                LlenaGridSubjetividad(False, 0, 0)

                SeleccionaBroker(hid_IndiceBroker.Value, gvd_Intermediario.Rows.Count - 1)
            Else
                Mensaje.MuestraMensaje("Mesa de Control", "No se ha seleccionado ningun elemento", TipoMsg.Advertencia)
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_RemoveCia_Click(sender As Object, e As EventArgs) Handles btn_RemoveCia.Click
        Try
            Dim strSel As String = ObtieneSeleccionados(gvd_Reasegurador, "chk_Sel", "cod_cia")

            If Len(strSel) > 0 Then
                Quita_Reasegurador(gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_grupo"),
                                   gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"),
                                   strSel)

                LlenaGridReasegurador(False, gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_grupo"),
                                      gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"))

                hid_IndiceReas.Value = IIf(gvd_Reasegurador.Rows.Count > 0, 0, -1)

                'Si se trata de un Negocio Directo
                If gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker") = 0 Then
                    LlenaGridSubjetividad(False, 0, 0)
                End If

                SeleccionaReasegurador(hid_IndiceReas.Value, gvd_Reasegurador.Rows.Count - 1)
            Else
                Mensaje.MuestraMensaje("Mesa de Control", "No se ha seleccionado ningun elemento", TipoMsg.Advertencia)
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_RemoveSubjetividad_Click(sender As Object, e As EventArgs) Handles btn_RemoveSubjetividad.Click
        Try
            Dim strSel As String = ObtieneSeleccionados(gvd_Subjetividad, "chk_Sel", "Num")
            Dim cod_cia As String = "0"


            If Len(strSel) > 0 Then

                If gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker") = 0 Then
                    If hid_IndiceReas.Value > -1 Then
                        cod_cia = gvd_Reasegurador.DataKeys(hid_IndiceReas.Value)("cod_cia")
                    End If
                End If

                Quita_Subjetividad(gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_grupo"),
                                   gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"),
                                   cod_cia,
                                   strSel)


                LlenaGridSubjetividad(False, gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_grupo"),
                                      gvd_Intermediario.DataKeys(hid_IndiceBroker.Value)("cod_broker"),
                                      cod_cia)
            Else
                Mensaje.MuestraMensaje("Mesa de Control", "No se ha seleccionado ningun elemento", TipoMsg.Advertencia)
            End If

        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_buscaPol_Click(sender As Object, e As ImageClickEventArgs) Handles btn_buscaPol.Click
        Try
            Dim ws As New ws_MesaControl.MesaControlClient
            dtPolizasAseg = New DataTable
            dtPolizasAseg = Funciones.Lista_A_Datatable(ws.ObtienePolizasAseg(txt_SearchAse.Text).ToList)
            If dtPolizasAseg.Rows.Count > 0 Then
                Funciones.LlenaGrid(gvd_AsegPolizas, dtPolizasAseg)
                Funciones.EjecutaFuncion("fn_AbrirModal('#PolizasAsegurado')", "Polizas")
            Else
                Mensaje.MuestraMensaje("Mesa de Control", "No existen Pólizas para el Asegurado", TipoMsg.Falla)
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_AceptarPol_Click(sender As Object, e As EventArgs) Handles btn_AceptarPol.Click
        Try
            Dim strSel = ObtieneSeleccionados(gvd_AsegPolizas, "chk_sel", "id_pv")
            If Len(strSel) > 0 Then

                Dim myRow() As Data.DataRow
                myRow = dtPolizasAseg.Select("id_pv IN (" & strSel & ")")

                If myRow.Length > 0 Then
                    txt_ClaveOfi.Text = myRow(0)("cod_suc")
                    txt_SearchOfi.Text = myRow(0)("sucursal")

                    txt_ClaveAseg.Text = myRow(0)("cod_aseg")
                    txt_SearchAse.Text = myRow(0)("asegurado")
                    txt_RFC.Text = myRow(0)("rfc")
                    ddl_Moneda.SelectedValue = myRow(0)("cod_moneda")
                    txt_ClaveTag.Text = myRow(0)("cod_tipo_agente")
                    txt_ClaveTagAux.Text = myRow(0)("cod_tipo_agente")
                    txt_SearchTag.Text = myRow(0)("tipo_agente")
                    txt_ClaveAge.Text = myRow(0)("cod_agente")
                    txt_SearchAge.Text = myRow(0)("agente")

                    txt_ClaveSusc.Text = myRow(0)("cod_usuario_suscriptor")
                    txt_SearchSusc.Text = myRow(0)("suscriptor")

                    txt_ClaveSuc.Text = myRow(0)("cod_suc")
                    txt_SearchSuc.Text = myRow(0)("sucursal")
                    txt_ClaveRam.Text = myRow(0)("cod_ramo")
                    txt_SearchRam.Text = myRow(0)("ramo")
                    txt_NroPoliza.Text = myRow(0)("nro_pol")
                    txt_Sufijo.Text = myRow(0)("aaaa_endoso")
                    txt_Endoso.Text = 0

                    txt_ClaveGre.Text = myRow(0)("cod_grupo_endo")
                    txt_ClaveGreAux.Text = myRow(0)("cod_grupo_endo")
                    txt_SearchGre.Text = myRow(0)("grupo_endoso")
                    txt_ClaveTte.Text = myRow(0)("cod_tipo_endo")
                    txt_SearchTte.Text = myRow(0)("tipo_endoso")

                    Funciones.EjecutaFuncion("fn_CerrarModal('#PolizasAsegurado')", "Polizas")
                End If
            End If

        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_InfoRiesgos_Click(sender As Object, e As ImageClickEventArgs) Handles btn_InfoRiesgos.Click
        Try
            Dim ws As New ws_MesaControl.MesaControlClient

            dtUbicaciones = New DataTable
            dtUbicaciones = Funciones.Lista_A_Datatable(ws.ObtieneUbicacionesPoliza(txt_ClaveSuc.Text, txt_ClaveRam.Text, txt_NroPoliza.Text, txt_Sufijo.Text, txt_Endoso.Text).ToList)

            If dtUbicaciones.Rows.Count > 0 Then
                dtRiesgosPol = New DataTable
                dtRiesgosPol = Funciones.Lista_A_Datatable(ws.ObtieneRiesgoPoliza(txt_ClaveSuc.Text, txt_ClaveRam.Text, txt_NroPoliza.Text, txt_Sufijo.Text, txt_Endoso.Text, dtUbicaciones.Rows(0)("cod_item"), "").ToList)
                If dtRiesgosPol.Rows.Count > 0 Then
                    Funciones.LlenaGrid(gvd_RiesgosPoliza, dtRiesgosPol)
                    ValidaRamosRiesgo(gvd_RiesgosPoliza, -1, {15, 16, 17, 18, 19, 20})

                    Dim ddl_Ubicacion As DropDownList = TryCast(gvd_RiesgosPoliza.HeaderRow.FindControl("ddl_Ubicacion"), DropDownList)
                    Funciones.LlenaDDL(ddl_Ubicacion, dtUbicaciones, "cod_item", "cod_item",)

                    lbl_RiesgoPoliza.Text = txt_ClaveSuc.Text & "-" & txt_ClaveRam.Text & "-" & txt_NroPoliza.Text & "-" & txt_Sufijo.Text & "-" & "0 >>> (" & txt_SearchAse.Text & ")"

                    Funciones.EjecutaFuncion("fn_AbrirModal('#RiesgosPoliza')", "Riesgos")
                Else
                    Mensaje.MuestraMensaje("Mesa de Control", "No existen Riesgos asociados a la Póliza", TipoMsg.Falla)
                End If
            End If

        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_AceptarRie_Click(sender As Object, e As EventArgs) Handles btn_AceptarRie.Click
        Try
            Dim inciso As Integer = 1
            If dtRiesgo.Rows.Count > 0 Then
                inciso = gvd_Riesgo.DataKeys(gvd_Riesgo.Rows.Count - 1)("cod_inciso") + 1
            End If

            fn_ActualizaDataRiesgo()

            Dim strSel = ObtieneSeleccionados(gvd_RiesgosPoliza, "chk_sel", "id")

            If Len(strSel) > 0 Then
                Dim myRow() As Data.DataRow
                myRow = dtRiesgosPol.Select("id IN (" & strSel & ")")
                For Each Row In myRow
                    dtRiesgo.Rows.Add(0, inciso, Row("cod_ramo"), Row("ramo_desc"), Row("cod_subramo"), Row("subramo_desc"),
                                      Row("cod_riesgo"), Row("riesgo_desc"), Row("cod_ind_cob"), Row("cobertura_desc"),
                                      Row("sn_facultativo"), Row("sn_adicional"), 0, Row("suma_asegurada"), Row("prima_neta"),
                                      Row("prima_inc"), Row("prima_fhm"), Row("prima_tev"), Row("prima_rc"), Row("prima_casco"), Row("prima_guerra"),
                                      Row("prc_com_age"), Row("com_agente"), Row("prc_com_adi_age"), Row("com_adi_agente"), Row("Cuota"), 0, 0, 0, 0)
                    inciso = inciso + 1
                Next
                LlenaGridRiesgo(False)
                Funciones.EjecutaFuncion("fn_CerrarModal('#RiesgosPoliza')", "Riesgos")

                Funciones.EjecutaFuncion("fn_SumaTotales('gvd_Riesgo', ['SumaAsegurada', 'PrimaNeta', 'PrimaINC', 'PrimaTEV', 'PrimaFHM', 'PrimaRC', 'PrimaCSC', 'PrimaGRA', 'Cuota'], ['txt_LimRespAux', 'txt_PrimaNetaAux', 'txt_PrimaINCAux', 'txt_PrimaTEVAux', 'txt_PrimaFHMAux', 'txt_PrimaRCAux', 'txt_PrimaCSCAux', 'txt_PrimaGRAAux', 'txt_CuotaAux'], 1, [0], 0);", "Sumatoria")
            End If

            Funciones.EjecutaFuncion("fn_CerrarModal('#EsperaModal');", "CerrarModal")
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_CerraPol_Click(sender As Object, e As EventArgs) Handles btn_CerraPol.Click
        Try
            dtPolizasAseg = Nothing
            Funciones.LlenaGrid(gvd_AsegPolizas, dtPolizasAseg)
            Funciones.EjecutaFuncion("fn_CerrarModal('#PolizasAsegurado')", "Polizas")
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_CerrarRie_Click(sender As Object, e As EventArgs) Handles btn_CerrarRie.Click
        Try
            dtRiesgosPol = Nothing
            dtUbicaciones = Nothing
            Funciones.LlenaGrid(gvd_RiesgosPoliza, dtRiesgosPol)
            Funciones.EjecutaFuncion("fn_CerrarModal('#RiesgosPoliza')", "Riesgos")
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_RemoveReparto_Click(sender As Object, e As EventArgs) Handles btn_RemoveReparto.Click
        Try
            Dim strSel As String = ObtieneSeleccionados(gvd_Reparto, "chk_Sel", "cod_reparto")

            If Len(strSel) > 0 Then
                Quita_Reparto(gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"), strSel)

                LlenaGridReparto(False, gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"))

                EjecutaCalculos()
            Else
                Mensaje.MuestraMensaje("Mesa de Control", "No se ha seleccionado ningun elemento", TipoMsg.Advertencia)
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub


    Protected Sub ddl_Ubicacion_SelectedIndexChanged(sender As Object, e As EventArgs)
        Try
            Dim ws As New ws_MesaControl.MesaControlClient
            dtRiesgosPol = New DataTable
            dtRiesgosPol = Funciones.Lista_A_Datatable(ws.ObtieneRiesgoPoliza(txt_ClaveSuc.Text, txt_ClaveRam.Text, txt_NroPoliza.Text, txt_Sufijo.Text, txt_Endoso.Text, sender.selectedValue, "").ToList)

            Funciones.LlenaGrid(gvd_RiesgosPoliza, dtRiesgosPol)
            ValidaRamosRiesgo(gvd_RiesgosPoliza, -1, {15, 16, 17, 18, 19, 20})

            Dim ddl_Ubicacion As DropDownList = TryCast(gvd_RiesgosPoliza.HeaderRow.FindControl("ddl_Ubicacion"), DropDownList)
            Funciones.LlenaDDL(ddl_Ubicacion, dtUbicaciones, "cod_item", "cod_item", sender.selectedValue)

        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_ConfirmaGrupo_Click(sender As Object, e As EventArgs) Handles btn_ConfirmaGrupo.Click
        Try
            Dim blnFacultativo As Boolean = False
            Dim grupo As Integer = 1
            Dim strRiesgos = "-1"
            Dim strRamos = "-1"
            Dim suma_asegurada, prima_neta, prima_inc, prima_tev, prima_fhm, prima_rc, prima_casco, prima_guerra As Double

            fn_ActualizaDataGrupo()

            If gvd_Agrupacion.Rows.Count > 0 Then
                grupo = gvd_Agrupacion.DataKeys(gvd_Agrupacion.Rows.Count - 1)("cod_grupo") + 1
            End If

            For Each row In gvd_Riesgo.Rows
                Dim chk_Riesgo As CheckBox = TryCast(row.FindControl("chk_Riesgo"), CheckBox)
                Dim opt_Facultativo As RadioButtonList = TryCast(row.FindControl("opt_Facultativo"), RadioButtonList)
                If chk_Riesgo.Checked = True And chk_Riesgo.Enabled = True And opt_Facultativo.SelectedValue = 1 Then
                    blnFacultativo = True
                    chk_Riesgo.Enabled = False
                    opt_Facultativo.Enabled = False
                    strRiesgos = strRiesgos & "," & gvd_Riesgo.DataKeys(row.RowIndex)("cod_inciso")
                    strRamos = strRamos & "," & TryCast(row.FindControl("txt_ClaveRamo"), TextBox).Text

                    suma_asegurada = suma_asegurada + IIf(TryCast(row.FindControl("opt_Adicional"), RadioButtonList).SelectedValue = 1, TryCast(row.FindControl("txt_LimRespAux"), TextBox).Text, 0)
                    prima_neta = prima_neta + IIf(TryCast(row.FindControl("txt_PrimaNetaAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaNetaAux"), TextBox).Text)
                    prima_inc = prima_inc + IIf(TryCast(row.FindControl("txt_PrimaINCAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaINCAux"), TextBox).Text)
                    prima_fhm = prima_fhm + IIf(TryCast(row.FindControl("txt_PrimaFHMAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaFHMAux"), TextBox).Text)
                    prima_tev = prima_tev + IIf(TryCast(row.FindControl("txt_PrimaTEVAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaTEVAux"), TextBox).Text)
                    prima_rc = prima_rc + IIf(TryCast(row.FindControl("txt_PrimaRCAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaRCAux"), TextBox).Text)
                    prima_casco = prima_casco + IIf(TryCast(row.FindControl("txt_PrimaCSCAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaCSCAux"), TextBox).Text)
                    prima_guerra = prima_guerra + IIf(TryCast(row.FindControl("txt_PrimaGRAAux"), TextBox).Text = vbNullString, 0, TryCast(row.FindControl("txt_PrimaGRAAux"), TextBox).Text)
                End If
            Next

            If blnFacultativo = True Then

                dtAgrupacion.Rows.Add(grupo, ddl_Agrupador.SelectedValue, ddl_Agrupador.SelectedItem.Text, suma_asegurada, prima_neta, prima_inc, prima_fhm, prima_tev, prima_rc, prima_casco, prima_guerra, strRiesgos, strRamos)

                LlenaGridAgrupacion(False)

                SeleccionaAgrupacion(hid_IndiceGrupo.Value, gvd_Agrupacion.Rows.Count - 1)

                Mensaje.MuestraMensaje("Mesa de Control", "Han sido agrupados los Riesgos: " & Replace(strRiesgos, "-1,", ""), TipoMsg.Confirma)
            Else
                Mensaje.MuestraMensaje("Mesa de Control", "No se ha habilitado ningún Riesgo como Facultativo, no se realizó ninguna Agrupación", TipoMsg.Advertencia)
            End If

            Funciones.EjecutaFuncion("fn_CerrarModal('#Agrupadores');", "CerrarModalAgrupación")
            Funciones.EjecutaFuncion("fn_CerrarModal('#EsperaModal');", "CerrarModal")

        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub Estado_Fila(ByRef gvd_Control As GridView, ByVal IndiceFila As Integer, ByVal Estatus As Integer)

        Select Case Estatus
            Case 0
                CType(gvd_Control.Rows(IndiceFila).FindControl("lnk_Clave"), LinkButton).BackColor = Drawing.Color.White
            Case 1
                CType(gvd_Control.Rows(IndiceFila).FindControl("lnk_Clave"), LinkButton).BackColor = Drawing.Color.LightGreen
            Case 2
                CType(gvd_Control.Rows(IndiceFila).FindControl("lnk_Clave"), LinkButton).BackColor = Drawing.Color.Orange
        End Select


        CType(gvd_Control.Rows(IndiceFila).FindControl("txt_PrcPart"), TextBox).Enabled = IIf(Estatus = 0, True, False)

        If gvd_Control.ID = "gvd_Intermediario" Then
            CType(gvd_Control.Rows(IndiceFila).FindControl("txt_PrcCorretaje"), TextBox).Enabled = IIf(Estatus = 0, True, False)
            CType(gvd_Control.Rows(IndiceFila).FindControl("txt_Corretaje"), TextBox).Enabled = IIf(Estatus = 0, True, False)


            For Each row In gvd_Reasegurador.Rows
                If hid_IndiceBroker.Value = IndiceFila Then
                    TryCast(row.FindControl("ddl_Estatus"), DropDownList).SelectedValue = Estatus
                    Estado_Fila(gvd_Reasegurador, row.rowIndex, Estatus)
                End If
            Next
        End If
    End Sub
    Protected Sub ddl_Estatus_SelectedIndexChanged(sender As Object, e As EventArgs)
        Try
            If InStr(sender.ClientID, "gvd_Intermediario") > 0 Then
                Estado_Fila(gvd_Intermediario, sender.NamingContainer.rowIndex, sender.selectedValue)
            Else
                Estado_Fila(gvd_Reasegurador, sender.NamingContainer.rowIndex, sender.selectedValue)
            End If

        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub chk_Proporcional_CheckedChanged(sender As Object, e As EventArgs) Handles chk_Proporcional.CheckedChanged
        Try
            If sender.checked = True Then
                gvd_Capas.Attributes.Add("style", "display:block")
                btn_AddCapa.Attributes.Remove("disabled")
                btn_RemoveCapa.Attributes.Remove("disabled")
            Else
                gvd_Capas.Attributes.Add("style", "display:none")
                btn_AddCapa.Attributes.Add("disabled", "true")
                btn_RemoveCapa.Attributes.Add("disabled", "true")
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_AddCapa_Click(sender As Object, e As EventArgs) Handles btn_AddCapa.Click
        Try
            Agrega_Capa()
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_RemoveCapa_Click(sender As Object, e As EventArgs) Handles btn_RemoveCapa.Click
        Try
            Dim strSel As String = ObtieneSeleccionados(gvd_Capas, "chk_Sel", "cod_capa")

            If Len(strSel) > 0 Then
                Quita_Capa(gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"), strSel)
                Renumera_Capa(gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"))
                LlenaGridCapas(False, gvd_Agrupacion.DataKeys(hid_IndiceGrupo.Value)("cod_grupo"))
            Else
                Mensaje.MuestraMensaje("Mesa de Control", "No se ha seleccionado ningun elemento", TipoMsg.Advertencia)
            End If
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub

    Private Sub btn_ConfirmaPrograma_Click(sender As Object, e As EventArgs) Handles btn_ConfirmaPrograma.Click
        Try
            Dim Estilo As String = "position:absolute;border:inset;border-color:black;border-width:1px;color:black;font-weight:bold;font-family:Myriad Pro;font-size:14px;"

            div_Capas.InnerHtml = "<div style=""width:100%;height:100px;top:0px;background-color:lightblue;" & Estilo & """> CAPA 7 --> $1,550,000,000 XS 2,000,000,000</div>" &
                                  "<div style=""width:100%;height:100px;top:100px;background-color:orange;" & Estilo & """>CAPA 6 --> $1,500,000,000 XS 500,000,000</div>" &
                                  "<div style=""width:35%;height:200px;top:200px;background-color:darkkhaki;" & Estilo & """>CAPA 5 --> $400,000,000 XS 100,000,000</div>" &
                                  "<div style=""width:65%;height:100px;top:200px;left:36.5%;background-color:blueviolet;" & Estilo & """>CAPA 4 --> $250,000,000 XS 250,000,000</div>" &
                                  "<div style=""width:42%;height:100px;top:300px;left:36.5%;background-color:khaki;" & Estilo & """>CAPA 3 --> $150,000,000 XS 100,000,000</div>" &
                                  "<div style=""width:23%;height:200px;top:300px;left:78.5%;background-color:yellowgreen;" & Estilo & """>CAPA 2 --> $250,000,000 XS DEDUCIBLES ORIGINALES</div>" &
                                  "<div style=""width:77%;height:100px;top:400px;background-color:lightcoral;" & Estilo & """>CAPA 1 --> $100,000,000 XS DEDUCIBLES ORIGINALES</div>"
        Catch ex As Exception
            Mensaje.MuestraMensaje("Mesa de Control", ex.Message, TipoMsg.Falla)
        End Try
    End Sub
End Class
