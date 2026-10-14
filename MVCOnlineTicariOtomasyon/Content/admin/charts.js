/* Ticari Otomasyon - Grafikler sayfası */
(function () {
  'use strict';

  var page = document.getElementById('chartPage');
  if (!page || !window.Chart) { return; }

  /* 1. Renkler ve yazı biçimleri */
  var css = getComputedStyle(document.documentElement);
  function token(name, fallback) { return (css.getPropertyValue(name) || '').trim() || fallback; }

  var INK = token('--ink', '#1B2420');
  var INK_2 = token('--ink-2', '#4B5952');
  var INK_3 = token('--ink-3', '#66746C');
  var GRID = '#E6EBE5';   // ızgara çizgileri
  var AXIS = token('--line-strong', '#BFCABF');
  var SURFACE = '#FFFFFF';   // kartın zemini
  var MAIN = token('--chart', '#1E8566');   // tek renkli grafiklerin rengi
  var PREVIOUS = '#A9B1AA';   // önceki dönem çizgisi

  // Kategori renkleri
  var CATEGORY_COLORS = ['#1E8566', '#2A78D6', '#EDA100', '#E87BA4', '#6250D6'];
  var OTHER_COLOR = '#C4C9C2';   // "Diğer" dilimi her zaman gri

  var money = new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', maximumFractionDigits: 2 });
  var moneyShort = new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', notation: 'compact', maximumFractionDigits: 1 });
  var number = new Intl.NumberFormat('tr-TR');
  var percent = new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 1 });

  function formatMoney(v) { return money.format(v); }
  function formatMoneyShort(v) { return Math.abs(v) >= 1000 ? moneyShort.format(v) : money.format(v); }

  function rgba(hex, alpha) {
    var n = parseInt(hex.slice(1), 16);
    return 'rgba(' + (n >> 16 & 255) + ',' + (n >> 8 & 255) + ',' + (n & 255) + ',' + alpha + ')';
  }

  // Bütün grafiklerin ortak ayarları
  Chart.defaults.font.family = '"IBM Plex Sans", "Segoe UI", system-ui, sans-serif';
  Chart.defaults.font.size = 12;
  Chart.defaults.color = INK_3;
  Chart.defaults.borderColor = GRID;
  Chart.defaults.animation.duration = 450;
  Chart.defaults.maintainAspectRatio = false;
  Chart.defaults.plugins.legend.display = false;   // açıklamalar HTML olarak kartın içinde

  /* 2. Eklentiler */

  // Çizgi grafikte fare hangi günün üstündeyse oraya dikey ince çizgi
  var crosshair = {
    id: 'crosshair',
    afterDatasetsDraw: function (chart) {
      var active = chart.tooltip && chart.tooltip.getActiveElements();
      if (!active || !active.length || chart.config.type !== 'line') { return; }
      var x = active[0].element.x, area = chart.chartArea, ctx = chart.ctx;
      ctx.save();
      ctx.beginPath();
      ctx.moveTo(x, area.top);
      ctx.lineTo(x, area.bottom);
      ctx.lineWidth = 1;
      ctx.strokeStyle = AXIS;
      ctx.stroke();
      ctx.restore();
    }
  };

  // Çubukların ucuna değeri yazar
  var valueLabels = {
    id: 'valueLabels',
    afterDatasetsDraw: function (chart, args, options) {
      if (!options || !options.format) { return; }
      var ctx = chart.ctx, horizontal = chart.options.indexAxis === 'y';
      var meta = chart.getDatasetMeta(0), data = chart.data.datasets[0].data;
      ctx.save();
      ctx.font = '600 12px ' + Chart.defaults.font.family;
      ctx.fillStyle = INK_2;
      meta.data.forEach(function (bar, i) {
        var text = options.format(data[i]);
        if (horizontal) {
          ctx.textAlign = 'left';
          ctx.textBaseline = 'middle';
          ctx.fillText(text, bar.x + 8, bar.y);
        } else {
          ctx.textAlign = 'center';
          ctx.textBaseline = 'bottom';
          ctx.fillText(text, bar.x, bar.y - 6);
        }
      });
      ctx.restore();
    }
  };

  Chart.register(crosshair, valueLabels);

  // HTML ipucu kutusu
  var tip = document.createElement('div');
  tip.className = 'chart-tip';
  tip.hidden = true;
  document.body.appendChild(tip);

  // rows -> { title
  function htmlTooltip(rows) {
    return function (context) {
      var model = context.tooltip;
      if (model.opacity === 0 || !model.dataPoints || !model.dataPoints.length) { tip.hidden = true; return; }

      var content = rows(context.chart, model.dataPoints[0].dataIndex);
      tip.textContent = '';
      var title = document.createElement('div');
      title.className = 'chart-tip__title';
      title.textContent = content.title;
      tip.appendChild(title);

      content.rows.forEach(function (r) {
        var row = document.createElement('div');
        row.className = 'chart-tip__row';
        var key = document.createElement('span');
        key.className = 'chart-tip__key' + (r.line ? ' chart-tip__key--line' : '');
        key.style.background = r.color;
        var value = document.createElement('strong');
        value.textContent = r.value;
        var label = document.createElement('span');
        label.textContent = r.label;
        row.appendChild(key);
        row.appendChild(value);
        row.appendChild(label);
        tip.appendChild(row);
      });

      tip.hidden = false;
      var box = context.chart.canvas.getBoundingClientRect();
      var left = box.left + model.caretX + 14;
      var top = box.top + model.caretY - tip.offsetHeight / 2;
      if (left + tip.offsetWidth > window.innerWidth - 8) { left = box.left + model.caretX - tip.offsetWidth - 14; }
      tip.style.left = Math.max(8, left) + 'px';
      tip.style.top = Math.max(8, Math.min(top, window.innerHeight - tip.offsetHeight - 8)) + 'px';
    };
  }
  window.addEventListener('scroll', function () { tip.hidden = true; }, true);

  function tooltipOptions(rows) {
    return { enabled: false, external: htmlTooltip(rows) };
  }

  // Değer ekseni
  function valueScale(extra) {
    var scale = {
      beginAtZero: true,
      grace: '8%',
      border: { display: false },
      grid: { color: GRID, drawTicks: false },
      ticks: { padding: 8, maxTicksLimit: 5, callback: function (v) { return formatMoneyShort(v); } }
    };
    return Object.assign(scale, extra || {});
  }

  function categoryScale(extra) {
    var scale = {
      border: { color: AXIS },
      grid: { display: false },
      ticks: { padding: 6, color: INK_2 }
    };
    return Object.assign(scale, extra || {});
  }

  function shorten(text, max) { return text.length > max ? text.slice(0, max - 1) + '…' : text; }
  function narrow() { return window.innerWidth < 576; }

  /* 3. Grafikler */
  var charts = {};
  function canvasOf(key) { return page.querySelector('[data-chart="' + key + '"] canvas'); }

  // Ciro çizgisi
  function drawTrend(data) {
    var points = data.points;
    var config = {
      type: 'line',
      data: {
        labels: points.map(function (p) { return p.label; }),
        datasets: [
          {
            label: 'Önceki dönem',
            data: points.map(function (p) { return p.previous; }),
            borderColor: PREVIOUS,
            borderWidth: 2,
            pointRadius: 0,
            pointHoverRadius: 4,
            pointHoverBackgroundColor: PREVIOUS,
            pointHoverBorderColor: SURFACE,
            pointHoverBorderWidth: 2,
            cubicInterpolationMode: 'monotone',   // yumuşak ama değerleri aşmayan eğri
            order: 2
          },
          {
            label: 'Bu dönem',
            data: points.map(function (p) { return p.current; }),
            borderColor: MAIN,
            borderWidth: 2,
            backgroundColor: rgba(MAIN, 0.1),
            fill: 'origin',
            pointRadius: 0,
            pointHoverRadius: 5,
            pointHoverBackgroundColor: MAIN,
            pointHoverBorderColor: SURFACE,
            pointHoverBorderWidth: 2,
            cubicInterpolationMode: 'monotone',   // yumuşak ama değerleri aşmayan eğri
            order: 1
          }
        ]
      },
      options: {
        interaction: { mode: 'index', intersect: false },
        layout: { padding: { top: 4, right: 8 } },
        scales: {
          x: categoryScale({ ticks: { color: INK_3, maxRotation: 0, autoSkipPadding: 18 } }),
          y: valueScale()
        },
        plugins: {
          tooltip: tooltipOptions(function (chart, i) {
            var p = points[i];
            return {
              title: p.title,
              rows: [
                { color: MAIN, line: true, value: formatMoney(p.current), label: 'bu dönem' },
                { color: PREVIOUS, line: true, value: formatMoney(p.previous), label: p.previousTitle }
              ]
            };
          })
        }
      }
    };
    render('trend', config, points.some(function (p) { return p.current > 0 || p.previous > 0; }));
  }

  // Ürünler
  function drawProducts(data) {
    var items = data.topProducts;
    var config = {
      type: 'bar',
      data: {
        labels: items.map(function (x) { return shorten(x.name, narrow() ? 14 : 24); }),
        datasets: [{
          data: items.map(function (x) { return x.revenue; }),
          backgroundColor: MAIN,
          hoverBackgroundColor: rgba(MAIN, 0.8),
          borderRadius: 4,
          borderSkipped: 'start',   // yuvarlak sadece uçta, başlangıç düz
          maxBarThickness: 22,
          categoryPercentage: 0.8
        }]
      },
      options: {
        indexAxis: 'y',
        layout: { padding: { right: 84 } },   // uçtaki değer yazısına yer
        scales: {
          x: valueScale({ display: false }),
          y: categoryScale({ border: { display: false } })
        },
        plugins: {
          valueLabels: { format: formatMoneyShort },
          tooltip: tooltipOptions(function (chart, i) {
            var x = items[i];
            return {
              title: x.name,
              rows: [
                { color: MAIN, value: formatMoney(x.revenue), label: 'ciro' },
                { color: 'transparent', value: number.format(x.quantity) + ' adet', label: 'satıldı' }
              ]
            };
          })
        }
      }
    };
    render('products', config, items.length > 0);
  }

  // Kategoriler
  function drawCategories(data) {
    var items = data.categories;
    var total = items.reduce(function (s, x) { return s + x.revenue; }, 0);
    var colors = items.map(function (x, i) { return x.other ? OTHER_COLOR : CATEGORY_COLORS[i % CATEGORY_COLORS.length]; });

    var config = {
      type: 'doughnut',
      data: {
        labels: items.map(function (x) { return x.name; }),
        datasets: [{
          data: items.map(function (x) { return x.revenue; }),
          backgroundColor: colors,
          borderColor: SURFACE,
          borderWidth: 2,   // dilimler arasında zemin renginde ince boşluk
          hoverOffset: 6
        }]
      },
      options: {
        cutout: '70%',
        layout: { padding: 6 },
        plugins: {
          tooltip: tooltipOptions(function (chart, i) {
            var x = items[i];
            return {
              title: x.name,
              rows: [{ color: colors[i], value: formatMoney(x.revenue), label: '%' + percent.format(x.revenue / total * 100) }]
            };
          })
        }
      }
    };

    page.querySelector('[data-bind="categoryTotal"]').textContent = formatMoneyShort(total);

    var list = page.querySelector('[data-bind="categoryList"]');
    list.textContent = '';
    items.forEach(function (x, i) {
      var li = document.createElement('li');
      li.className = 'share-list__item';
      li.tabIndex = 0;
      li.innerHTML = '<span class="share-list__swatch"></span><span class="share-list__name"></span>' +
                     '<span class="share-list__pct"></span><span class="share-list__value"></span>';
      li.querySelector('.share-list__swatch').style.background = colors[i];
      li.querySelector('.share-list__name').textContent = x.name;
      li.querySelector('.share-list__pct').textContent = '%' + percent.format(total ? x.revenue / total * 100 : 0);
      li.querySelector('.share-list__value').textContent = formatMoney(x.revenue);

      // Listede bir satırın üstüne gelince halkadaki dilimi de vurgula
      function highlight(on) {
        var chart = charts.categories;
        if (!chart) { return; }
        chart.setActiveElements(on ? [{ datasetIndex: 0, index: i }] : []);
        chart.update('none');
      }
      li.addEventListener('mouseenter', function () { highlight(true); });
      li.addEventListener('mouseleave', function () { highlight(false); });
      li.addEventListener('focus', function () { highlight(true); });
      li.addEventListener('blur', function () { highlight(false); });
      list.appendChild(li);
    });

    render('categories', config, total > 0);
  }

  // Personeller
  function drawPersonnel(data) {
    var items = data.personnel;
    var config = {
      type: 'bar',
      data: {
        labels: items.map(function (x) { return x.name.split(' ').map(function (w) { return shorten(w, 12); }); }),   // ad ve soyad alt alta
        datasets: [{
          data: items.map(function (x) { return x.revenue; }),
          backgroundColor: MAIN,
          hoverBackgroundColor: rgba(MAIN, 0.8),
          borderRadius: 4,
          borderSkipped: 'start',
          maxBarThickness: 24
        }]
      },
      options: {
        layout: { padding: { top: 22 } },   // tepedeki değer yazısına yer
        scales: {
          x: categoryScale({ ticks: { padding: 6, color: INK_2, maxRotation: 0, autoSkip: false } }),
          y: valueScale({ display: false })
        },
        plugins: {
          valueLabels: { format: formatMoneyShort },
          tooltip: tooltipOptions(function (chart, i) {
            var x = items[i];
            return {
              title: x.name,
              rows: [
                { color: MAIN, value: formatMoney(x.revenue), label: 'ciro' },
                { color: 'transparent', value: number.format(x.count) + ' satış', label: 'yaptı' }
              ]
            };
          })
        }
      }
    };
    render('personnel', config, items.length > 0);
  }

  // Grafiği ilk kez çizer ya da var olanın verisini değiştirir
  function render(key, config, hasData) {
    var card = page.querySelector('[data-chart="' + key + '"]');
    card.querySelector('.chart-box').hidden = !hasData;
    card.querySelector('.chart-card__empty').hidden = hasData;
    var list = card.querySelector('.share-list');
    if (list) { list.hidden = !hasData; }

    if (charts[key]) {
      charts[key].data = config.data;
      charts[key].options = config.options;
      charts[key].update();
    } else {
      charts[key] = new Chart(canvasOf(key), config);
    }
  }

  /* 4. Özet kartları ve tablolar */
  function drawKpis(data) {
    Object.keys(data.tiles).forEach(function (key) {
      var tile = data.tiles[key];
      var valueEl = page.querySelector('[data-kpi="' + key + '"]');
      valueEl.textContent = valueEl.getAttribute('data-format') === 'money' ? formatMoney(tile.value) : number.format(tile.value);

      var delta = page.querySelector('[data-delta="' + key + '"]');
      delta.className = 'kpi__delta';
      if (tile.change === null) {
        delta.textContent = 'Önceki dönemde satış yok';
        return;
      }
      var up = tile.change > 0, flat = tile.change === 0;
      delta.classList.add(flat ? 'is-flat' : up ? 'is-up' : 'is-down');
      // Yön hem okla hem işaretle yazılır
      delta.textContent = (flat ? '→ ' : up ? '↑ ' : '↓ ') + '%' + percent.format(Math.abs(tile.change)) + ' ' + data.compareLabel;
    });
  }

  function table(card, head, rows) {
    var wrap = page.querySelector('[data-chart="' + card + '"] .chart-table .table-responsive');
    var t = document.createElement('table');
    t.className = 'table table-sm mb-0';
    t.setAttribute('data-admin', 'off');
    var thead = t.createTHead().insertRow();
    head.forEach(function (h, i) {
      var th = document.createElement('th');
      th.textContent = h;
      if (i > 0) { th.className = 'cell-num'; }
      thead.appendChild(th);
    });
    var body = t.createTBody();
    rows.forEach(function (r) {
      var tr = body.insertRow();
      r.forEach(function (v, i) {
        var td = tr.insertCell();
        td.textContent = v;
        if (i > 0) { td.className = 'cell-num'; }
      });
    });
    wrap.textContent = '';
    wrap.appendChild(t);
  }

  function drawTables(data) {
    table('trend', [data.monthly ? 'Ay' : 'Gün', 'Bu dönem', 'Önceki dönem'],
      data.points.map(function (p) { return [p.title, formatMoney(p.current), formatMoney(p.previous)]; }));
    table('products', ['Ürün', 'Adet', 'Ciro'],
      data.topProducts.map(function (x) { return [x.name, number.format(x.quantity), formatMoney(x.revenue)]; }));
    table('categories', ['Kategori', 'Ciro'],
      data.categories.map(function (x) { return [x.name, formatMoney(x.revenue)]; }));
    table('personnel', ['Personel', 'Satış', 'Ciro'],
      data.personnel.map(function (x) { return [x.name, number.format(x.count), formatMoney(x.revenue)]; }));
  }

  /* 5. Dönem düğmeleri ve yükleme */
  var url = page.getAttribute('data-url');
  var error = page.querySelector('.chart-error');
  var request = 0;

  function load(days) {
    var id = ++request;
    page.classList.add('is-loading');   // yenilenirken eski çizim soluk kalır, sayfa zıplamaz
    error.hidden = true;

    fetch(url + '?days=' + days, { headers: { 'X-Requested-With': 'XMLHttpRequest' }, credentials: 'same-origin' })
      .then(function (response) {
        if (!response.ok) { throw new Error(response.status); }
        return response.json();
      })
      .then(function (data) {
        if (id !== request) { return; }   // arada başka dönem seçildiyse eski cevabı at
        page.querySelector('[data-bind="periodLabel"]').textContent = data.periodLabel;
        page.querySelector('[data-bind="trendTitle"]').textContent = data.monthly ? 'Aylık ciro' : 'Günlük ciro';
        drawKpis(data);
        drawTrend(data);
        drawProducts(data);
        drawCategories(data);
        drawPersonnel(data);
        drawTables(data);
      })
      .catch(function () { if (id === request) { error.hidden = false; } })
      .then(function () { if (id === request) { page.classList.remove('is-loading'); } });
  }

  page.querySelectorAll('.segmented__item').forEach(function (button) {
    button.addEventListener('click', function (e) {
      e.preventDefault();
      var days = button.getAttribute('data-days');
      page.querySelectorAll('.segmented__item').forEach(function (b) {
        var on = b === button;
        b.classList.toggle('is-active', on);
        b.setAttribute('aria-pressed', on ? 'true' : 'false');
      });
      // Adres de değişsin
      if (window.history.replaceState) { window.history.replaceState(null, '', button.getAttribute('href')); }
      load(days);
    });
  });

  load(page.getAttribute('data-days'));
})();
