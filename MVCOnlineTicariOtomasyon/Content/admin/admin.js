/* Ticari Otomasyon - Admin teması */
(function () {
  'use strict';

  var root = document.documentElement;
  var STORAGE_KEY = 'to.sidebar';
  var desktop = window.matchMedia('(min-width: 992px)');

  /* 1. Yan menü */
  var toggle = document.getElementById('sidebarToggle');
  var overlay = document.querySelector('.app-overlay');

  function closeMobileMenu() {
    root.classList.remove('sidebar-open');
  }

  if (toggle) {
    toggle.addEventListener('click', function () {
      if (desktop.matches) {
        var collapsed = root.classList.toggle('sidebar-collapsed');
        try { localStorage.setItem(STORAGE_KEY, collapsed ? 'collapsed' : 'open'); } catch (e) { /* gizli sekme vb */ }
      } else {
        root.classList.toggle('sidebar-open');
      }
    });
  }

  if (overlay) {
    overlay.addEventListener('click', closeMobileMenu);
  }

  document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') { closeMobileMenu(); }
  });

  // Ekran genişleyip masaüstüne geçince mobil menü açık kalmasın
  if (desktop.addEventListener) {
    desktop.addEventListener('change', closeMobileMenu);
  }

  /* 2. Tablolar */
  var tables = Array.prototype.filter.call(document.querySelectorAll('.app-content table.table'), function (table) {
    return table.getAttribute('data-admin') !== 'off';
  });

  tables.forEach(function (table) {
    if (table.closest('.table-responsive')) { return; }
    var wrapper = document.createElement('div');
    wrapper.className = 'table-responsive';
    table.parentNode.insertBefore(wrapper, table);
    wrapper.appendChild(table);
  });

  /* 3. Arama */
  var input = document.getElementById('tableSearch');
  var box = input ? input.closest('.app-search') : null;
  var searchable = tables.filter(function (t) { return t.getAttribute('data-search') !== 'off'; });

  // Türkçe büyük/küçük harf kuralı
  function normalize(text) {
    return text.toLocaleLowerCase('tr-TR');
  }

  function dataRows(table) {
    return Array.prototype.filter.call(table.querySelectorAll('tbody tr'), function (row) {
      // Başlık satırını ve "sonuç yok" satırını atla
      return !row.querySelector('th') && !row.classList.contains('app-empty-row');
    });
  }

  // Satırın aranacak metni
  function rowText(row) {
    return Array.prototype.map.call(row.cells, function (cell) {
      return cell.classList.contains('cell-actions') ? '' : cell.textContent;
    }).join(' ');
  }

  function emptyRow(table) {
    var row = table.querySelector('tr.app-empty-row');
    if (!row) {
      var firstRow = table.querySelector('tr');
      var cell = document.createElement('td');
      cell.colSpan = firstRow ? firstRow.children.length : 1;
      cell.textContent = 'Aramayla eşleşen kayıt yok.';
      row = document.createElement('tr');
      row.className = 'app-empty-row';
      row.hidden = true;
      row.appendChild(cell);
      (table.tBodies[0] || table).appendChild(row);
    }
    return row;
  }

  // Tablo dışındaki listeler
  var ITEM_SELECTOR = '.app-content [data-search-item]';
  var emptyNote = null;

  function filterItems(query) {
    var items = Array.prototype.slice.call(document.querySelectorAll(ITEM_SELECTOR));
    if (!items.length) { return; }
    var visible = 0;

    items.forEach(function (item) {
      var text = item.getAttribute('data-search-item') || item.textContent;
      var match = !query || normalize(text).indexOf(query) !== -1;
      item.hidden = !match;
      if (match) { visible++; }
    });

    if (!emptyNote) {
      var list = items[0].parentNode;
      emptyNote = document.createElement('p');
      emptyNote.className = 'app-empty-note';
      emptyNote.textContent = 'Aramayla eşleşen kayıt yok.';
      list.parentNode.insertBefore(emptyNote, list.nextSibling);
    }
    emptyNote.hidden = visible > 0;
  }

  function filterTables() {
    var query = normalize(input.value.trim());
    filterItems(query);

    searchable.forEach(function (table) {
      var rows = dataRows(table);
      var visible = 0;

      rows.forEach(function (row) {
        var match = !query || normalize(rowText(row)).indexOf(query) !== -1;
        row.hidden = !match;
        if (match) { visible++; }
      });

      if (rows.length) {
        emptyRow(table).hidden = visible > 0;
      }
    });
  }

  if (input && box && (searchable.length || document.querySelector(ITEM_SELECTOR))) {
    box.hidden = false;
    input.addEventListener('input', filterTables);

    document.addEventListener('keydown', function (e) {
      var tag = (e.target.tagName || '').toLowerCase();
      var typing = tag === 'input' || tag === 'textarea' || tag === 'select' || e.target.isContentEditable;

      // "/" tuşu aramaya odaklanır
      if (e.key === '/' && !typing && !e.ctrlKey && !e.metaKey && !e.altKey) {
        e.preventDefault();
        input.focus();
      }
      // Arama kutusundayken Esc aramayı temizler
      if (e.key === 'Escape' && e.target === input) {
        input.value = '';
        filterTables();
        input.blur();
      }
    });
  }

  /* 4. Silme onayı */
  var deleteModal = document.getElementById('deleteModal');

  if (deleteModal && window.jQuery) {
    window.jQuery(deleteModal).on('show.bs.modal', function (e) {
      var trigger = e.relatedTarget;
      if (!trigger) { return; }

      var name = trigger.getAttribute('data-delete-name');
      deleteModal.querySelector('form').setAttribute('action', trigger.getAttribute('data-delete-url'));
      deleteModal.querySelector('.modal-title').textContent = name ? '\u201C' + name + '\u201D silinsin mi?' : 'Kayıt silinsin mi?';
      deleteModal.querySelector('.modal-body').textContent = trigger.getAttribute('data-delete-text') || 'Bu işlem geri alınamaz.';
    });
  }

  /* 5. Görseller */
  function removeImage(img) {
    if (img.parentNode) { img.parentNode.removeChild(img); }
  }

  // "error" olayı yukarı taşmadığı için yakalama aşamasında dinlenir
  document.addEventListener('error', function (e) {
    var el = e.target;
    if (el && el.tagName === 'IMG' && el.closest('.thumb')) { removeImage(el); }
  }, true);

  // Bu dosya çalışmadan önce yüklenmeyi deneyip başaramamış görseller
  Array.prototype.forEach.call(document.querySelectorAll('.thumb img'), function (img) {
    if (img.complete && img.naturalWidth === 0) { removeImage(img); }
  });

  // Formda canlı önizleme
  Array.prototype.forEach.call(document.querySelectorAll('input[data-preview]'), function (field) {
    var thumb = document.querySelector(field.getAttribute('data-preview'));
    if (!thumb) { return; }

    function clear() {
      var current = thumb.querySelector('img');
      if (current) { removeImage(current); }
    }

    if (field.type === 'file') {
      filePreview(field, thumb, clear);
      return;
    }

    function preview() {
      var url = field.value.trim();
      clear();
      if (!url) { return; }

      var img = new Image();
      img.alt = '';
      img.onload = function () {
        // Yüklenirken adres değiştiyse eski görseli gösterme
        if (field.value.trim() === url) { clear(); thumb.appendChild(img); }
      };
      img.src = url;
    }

    field.addEventListener('input', preview);
    preview();
  });

  // Dosya seçme kutusu
  function filePreview(field, thumb, clear) {
    var original = thumb.querySelector('img');   // sayfa açıldığındaki fotoğraf
    var nameBox = field.closest('.file-pick') ? field.closest('.file-pick').querySelector('[data-file-name]') : null;
    var emptyText = nameBox ? nameBox.textContent : '';
    var maxBytes = Number(field.getAttribute('data-max-bytes')) || 0;
    var objectUrl = null;

    function showName(text, isError) {
      if (!nameBox) { return; }
      nameBox.textContent = text;
      nameBox.classList.toggle('is-error', !!isError);
    }

    field.addEventListener('change', function () {
      var file = field.files && field.files[0];
      if (objectUrl) { URL.revokeObjectURL(objectUrl); objectUrl = null; }
      clear();

      if (!file) {   // seçim iptal edildi
        if (original) { thumb.appendChild(original); }
        showName(emptyText);
        return;
      }

      if (maxBytes && file.size > maxBytes) {
        field.value = '';
        if (original) { thumb.appendChild(original); }
        showName('“' + file.name + '” çok büyük (' + (file.size / 1048576).toFixed(1).replace('.', ',') + ' MB). En fazla ' + (maxBytes / 1048576) + ' MB.', true);
        return;
      }

      showName(file.name);
      objectUrl = URL.createObjectURL(file);
      var img = new Image();
      img.alt = '';
      img.src = objectUrl;
      thumb.appendChild(img);
    });
  }

  /* 6. İpucu balonu */
  var tip = null;
  var tipOwner = null;

  function hideTip() {
    if (tip) { tip.hidden = true; }
    tipOwner = null;
  }

  function showTip(owner) {
    if (!tip) {
      tip = document.createElement('div');
      tip.className = 'app-tip';
      tip.setAttribute('role', 'tooltip');
      tip.appendChild(document.createElement('strong'));
      tip.appendChild(document.createElement('span'));
      document.body.appendChild(tip);
    }

    tipOwner = owner;
    tip.firstChild.textContent = owner.getAttribute('data-tip-value');
    tip.lastChild.textContent = owner.getAttribute('data-tip-label') || '';
    tip.hidden = false;

    var mark = owner.querySelector('.cols__bar') || owner;
    var box = mark.getBoundingClientRect();
    var left = box.left + box.width / 2 - tip.offsetWidth / 2;
    var top = box.top - tip.offsetHeight - 8;

    // Ekranın dışına taşmasın
    left = Math.max(8, Math.min(left, window.innerWidth - tip.offsetWidth - 8));
    if (top < 8) { top = box.bottom + 8; }

    tip.style.left = left + 'px';
    tip.style.top = top + 'px';
  }

  function tipTarget(node) {
    return node && node.closest ? node.closest('[data-tip-value]') : null;
  }

  document.addEventListener('mouseover', function (e) {
    var owner = tipTarget(e.target);
    if (owner && owner !== tipOwner) { showTip(owner); }
  });
  document.addEventListener('mouseout', function (e) {
    var owner = tipTarget(e.target);
    if (owner && !owner.contains(e.relatedTarget)) { hideTip(); }
  });
  document.addEventListener('focusin', function (e) {
    var owner = tipTarget(e.target);
    if (owner) { showTip(owner); }
  });
  document.addEventListener('focusout', hideTip);
  window.addEventListener('scroll', hideTip, true);
})();
