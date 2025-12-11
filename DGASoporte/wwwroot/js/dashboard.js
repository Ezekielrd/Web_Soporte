// wwwroot/js/dashboard.js
(function (window) {

    let chartSolicitudes = null;
    let chartCargaTecnicos = null;
    let chartEstados = null;
    let chartUnidades = null;

    // ================== GRÁFICOS ==================
    function cargarDatosDashboard(root) {
        // Si ya está cargado (por ejemplo al navegar entre filtros) no hacemos nada
        if (window.DashboardData && Object.keys(window.DashboardData).length) {
            return;
        }

        // 👇 buscar en todo el documento, no dentro de dashboardRoot
        const script = document.getElementById('dashboard-data');
        if (!script) {
            console.warn('[Dashboard] No se encontró #dashboard-data');
            return;
        }

        try {
            const json = script.textContent || script.innerText || '{}';
            const data = JSON.parse(json);
            window.DashboardData = data;
            // console.log('[Dashboard] Datos cargados', data);
        } catch (err) {
            console.error('[Dashboard] Error parseando dashboard-data', err);
        }
    }


    function crearChartSolicitudes(root) {
        if (!window.Chart || !window.DashboardData) return;
        const canvas = root.querySelector('#chartSolicitudesMes');
        if (!canvas) return;

        const ctx = canvas.getContext('2d');
        const d = window.DashboardData;

        if (chartSolicitudes) {
            chartSolicitudes.destroy();
        }

        chartSolicitudes = new Chart(ctx, {
            type: 'line',
            data: {
                labels: d.mesesLabels || [],
                datasets: [
                    {
                        label: 'Solicitudes creadas',
                        data: d.mesesSolicitudes || [],
                        tension: 0.3,
                        borderWidth: 2,
                        fill: false,
                        borderColor: 'rgba(37, 99, 235, 1)',
                        pointRadius: 3
                    },
                    {
                        label: 'Solicitudes cerradas',
                        data: d.mesesCerradas || [],
                        tension: 0.3,
                        borderWidth: 2,
                        fill: false,
                        borderColor: 'rgba(34, 197, 94, 1)',
                        pointRadius: 3
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    mode: 'index',
                    intersect: false
                },
                plugins: {
                    legend: { display: true },
                    tooltip: { enabled: true }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: { precision: 0 }
                    }
                }
            }
        });
    }

    function crearChartCargaTecnicos(root) {
        if (!window.Chart || !window.DashboardData) return;
        const canvas = root.querySelector('#chartCargaTecnicos');
        if (!canvas) return;

        const ctx = canvas.getContext('2d');
        const d = window.DashboardData;

        if (chartCargaTecnicos) {
            chartCargaTecnicos.destroy();
        }

        chartCargaTecnicos = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: d.tecnicosLabels || [],
                datasets: [
                    {
                        label: 'Abiertas',
                        data: d.tecnicosAbiertas || [],
                        backgroundColor: 'rgba(249, 115, 22, 0.8)'
                    },
                    {
                        label: 'Cerradas (30 días)',
                        data: d.tecnicosCerradas || [],
                        backgroundColor: 'rgba(34, 197, 94, 0.8)'
                    }
                ]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: true },
                    tooltip: { enabled: true }
                },
                scales: {
                    x: {
                        beginAtZero: true,
                        ticks: { precision: 0 }
                    }
                }
            }
        });
    }

    function crearChartEstados(root) {
        if (!window.Chart || !window.DashboardData) return;
        const canvas = root.querySelector('#chartEstadosSolicitud');
        if (!canvas) return;

        const ctx = canvas.getContext('2d');
        const d = window.DashboardData;

        if (chartEstados) {
            chartEstados.destroy();
        }

        chartEstados = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: d.estadosLabels || [],
                datasets: [{
                    data: d.estadosValores || [],
                    backgroundColor: [
                        'rgba(59, 130, 246, 0.8)',
                        'rgba(34, 197, 94, 0.8)',
                        'rgba(239, 68, 68, 0.8)',
                        'rgba(234, 179, 8, 0.8)'
                    ]
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'bottom' }
                }
            }
        });
    }

    function crearChartUnidades(root) {
        if (!window.Chart || !window.DashboardData) return;
        const canvas = root.querySelector('#chartTopUnidades');
        if (!canvas) return;

        const ctx = canvas.getContext('2d');
        const d = window.DashboardData;

        if (chartUnidades) {
            chartUnidades.destroy();
        }

        chartUnidades = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: d.unidadesLabels || [],
                datasets: [{
                    label: 'Solicitudes',
                    data: d.unidadesValores || [],
                    backgroundColor: 'rgba(37, 99, 235, 0.8)'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: { enabled: true }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: { precision: 0 }
                    }
                }
            }
        });
    }

    function renderCharts(dashboardRoot) {
        crearChartSolicitudes(dashboardRoot);
        crearChartCargaTecnicos(dashboardRoot);
        crearChartEstados(dashboardRoot);
        crearChartUnidades(dashboardRoot);
    }

    // ================== FILTROS (opcionales) ==================

    function setupFiltros(dashboardRoot) {
        const filtros = dashboardRoot.querySelector('.js-dashboard-filtros');
        if (!filtros) return; // esta vista (gerencial) aún no tiene filtros → solo gráficos

        const viewContainer = document.getElementById('view-container');
        if (!viewContainer) return;

        const urlBase = filtros.dataset.urlBase;
        const modoInicial = (dashboardRoot.dataset.modo || 'dia').toLowerCase();
        const fechaInicialStr = dashboardRoot.dataset.fecha;
        const fechaInicial = fechaInicialStr ? new Date(fechaInicialStr) : new Date();
        const yearBase = fechaInicial.getFullYear();

        const selMes = filtros.querySelector('#filtroMes');
        const selSemana = filtros.querySelector('#filtroSemana');
        const selDia = filtros.querySelector('#filtroDia');

        if (!urlBase || !selMes || !selSemana || !selDia) return;

        let isInitializing = true;

        function getWeeksOfMonth(year, month) {
            const weeks = [];
            const firstDay = new Date(year, month - 1, 1);
            const lastDay = new Date(year, month, 0);

            let current = new Date(firstDay);
            const dow = current.getDay(); // 0-dom,1-lun...
            const diff = (dow === 0 ? -6 : 1 - dow);
            current.setDate(current.getDate() + diff);

            while (current <= lastDay) {
                const weekStart = new Date(current);
                const weekEnd = new Date(weekStart);
                weekEnd.setDate(weekEnd.getDate() + 6);
                weeks.push({ start: weekStart, end: weekEnd });
                current.setDate(current.getDate() + 7);
            }
            return weeks;
        }

        function rellenarSemanas(mes) {
            const weeks = getWeeksOfMonth(yearBase, mes);
            selSemana.innerHTML = '';
            selSemana.appendChild(new Option('Todo el mes', '', true, true));

            weeks.forEach((w, i) => {
                const label = `${w.start.toLocaleDateString('es-NI')} - ${w.end.toLocaleDateString('es-NI')}`;
                selSemana.appendChild(new Option(`Semana ${i + 1}: ${label}`, String(i)));
            });
        }

        function rellenarDias(mes, weekIndex) {
            selDia.innerHTML = '';
            selDia.appendChild(new Option('Toda la semana', '', true, true));

            if (weekIndex === '') return;

            const weeks = getWeeksOfMonth(yearBase, mes);
            const w = weeks[parseInt(weekIndex, 10)];
            if (!w) return;

            const start = new Date(w.start);
            for (let i = 0; i < 7; i++) {
                const d = new Date(start);
                d.setDate(start.getDate() + i);
                if (d.getMonth() !== mes - 1) break;

                const value = d.toISOString().substring(0, 10);
                const label = d.toLocaleDateString('es-NI', {
                    weekday: 'short',
                    day: '2-digit'
                });

                selDia.appendChild(new Option(label, value));
            }
        }

        function inicializarSeleccion() {
            const mesInicial = fechaInicial.getMonth() + 1;
            selMes.value = String(mesInicial);

            rellenarSemanas(mesInicial);

            const weeks = getWeeksOfMonth(yearBase, mesInicial);
            let idxSemana = -1;
            weeks.forEach((w, i) => {
                if (fechaInicial >= w.start && fechaInicial <= w.end) {
                    idxSemana = i;
                }
            });

            if (modoInicial === 'mes') {
                selSemana.value = '';
                rellenarDias(mesInicial, '');
                selDia.value = '';
            } else {
                if (idxSemana >= 0) {
                    selSemana.value = String(idxSemana);
                } else {
                    selSemana.value = '';
                }
                rellenarDias(mesInicial, selSemana.value);

                if (modoInicial === 'dia') {
                    const iso = fechaInicial.toISOString().substring(0, 10);
                    selDia.value = iso;
                } else {
                    selDia.value = '';
                }
            }

            isInitializing = false;
        }

        async function aplicarFiltros() {
            const mes = parseInt(selMes.value, 10);
            const weeks = getWeeksOfMonth(yearBase, mes);
            const weekIndex = selSemana.value;
            const diaValue = selDia.value;

            let modo, fechaRef;

            if (diaValue) {
                modo = 'dia';
                fechaRef = diaValue;
            } else if (weekIndex !== '') {
                modo = 'semana';
                const w = weeks[parseInt(weekIndex, 10)];
                fechaRef = w.start.toISOString().substring(0, 10);
            } else {
                modo = 'mes';
                const f = new Date(yearBase, mes - 1, 1);
                fechaRef = f.toISOString().substring(0, 10);
            }

            const url = `${urlBase}?modo=${encodeURIComponent(modo)}&fecha=${encodeURIComponent(fechaRef)}`;

            try {
                const resp = await fetch(url, {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });
                const html = await resp.text();
                viewContainer.innerHTML = html;

                if (window.Dashboard && typeof window.Dashboard.init === 'function') {
                    window.Dashboard.init(viewContainer);
                }
            } catch (err) {
                console.error('[Dashboard] Error al aplicar filtros', err);
            }
        }

        inicializarSeleccion();

        selMes.addEventListener('change', () => {
            const mes = parseInt(selMes.value, 10);
            rellenarSemanas(mes);
            rellenarDias(mes, '');
            if (!isInitializing) aplicarFiltros();
        });

        selSemana.addEventListener('change', () => {
            const mes = parseInt(selMes.value, 10);
            rellenarDias(mes, selSemana.value);
            if (!isInitializing) aplicarFiltros();
        });

        selDia.addEventListener('change', () => {
            if (!isInitializing) aplicarFiltros();
        });
    }

    // ================== INIT ==================

    function initDashboard(root) {
        root = root || document;

        const dashboardRoot = root.querySelector('#dashboard-root');
        if (!dashboardRoot) return;

        // 1) Cargar datos desde el JSON embebido
        cargarDatosDashboard(dashboardRoot);

        // 2) Dibujar gráficos
        renderCharts(dashboardRoot);

        // 3) Configurar filtros (si existieran)
        setupFiltros(dashboardRoot);
    }


    document.addEventListener('DOMContentLoaded', function () {
        initDashboard(document);
    });

    window.Dashboard = window.Dashboard || {};
    window.Dashboard.init = initDashboard;

})(window);
