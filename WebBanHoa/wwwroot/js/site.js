/* FLORIA – hiệu ứng & tương tác phía người dùng (Vanilla JS, không phụ thuộc thư viện) */
(() => {
  const $ = (s, el = document) => el.querySelector(s);
  const $$ = (s, el = document) => [...el.querySelectorAll(s)];
  const reduce = matchMedia("(prefers-reduced-motion: reduce)").matches;
  const token = () => $('input[name="__RequestVerificationToken"]')?.value || "";

  /* ---------- Toast ---------- */
  window.toast = (msg, html = "") => {
    const wrap = $(".toast-wrap");
    if (!wrap || !msg) return;
    const t = document.createElement("div");
    t.className = "toast";
    t.innerHTML = `<span class="ti">✿</span><div>${msg}${html}</div>`;
    wrap.appendChild(t);
    setTimeout(() => { t.classList.add("out"); setTimeout(() => t.remove(), 400); }, 3600);
  };
  const initial = document.body.dataset.toast;
  if (initial) setTimeout(() => toast(initial), 300);

  /* ---------- Hiệu ứng cuộn: ẩn/hiện phần tử ---------- */
  // Tách tiêu đề thành từng dòng để chạy hiệu ứng "mask" từng chữ
  $$("[data-split]").forEach(el => {
    const words = el.innerHTML.split(/<br\s*\/?>/i);
    el.innerHTML = words.map((w, i) => `<span class="split-line"><span style="--d:${i * 0.12}s">${w}</span></span>`).join("");
    el.classList.add("reveal"); el.dataset.reveal = "fade";
  });
  // Tự động gán độ trễ tăng dần cho các phần tử con trong [data-stagger]
  $$("[data-stagger]").forEach(group => {
    const step = parseFloat(group.dataset.stagger) || 0.08;
    [...group.children].forEach((c, i) => {
      c.classList.add("reveal");
      if (!c.dataset.reveal) c.dataset.reveal = group.dataset.revealChild || "up";
      c.style.setProperty("--d", `${(i % 8) * step}s`);
    });
  });
  const io = new IntersectionObserver(entries => {
    entries.forEach(e => {
      if (e.isIntersecting) { e.target.classList.add("in"); io.unobserve(e.target); }
    });
  }, { threshold: 0.14, rootMargin: "0px 0px -40px 0px" });
  $$(".reveal, [data-reveal]").forEach(el => { el.classList.add("reveal"); io.observe(el); });

  /* ---------- Đếm số ---------- */
  const countIO = new IntersectionObserver(entries => entries.forEach(e => {
    if (!e.isIntersecting) return;
    const el = e.target, end = parseFloat(el.dataset.count), suffix = el.dataset.suffix || "";
    const t0 = performance.now(), dur = 1600;
    const tick = now => {
      const p = Math.min(1, (now - t0) / dur), v = end * (1 - Math.pow(1 - p, 3));
      el.textContent = (end % 1 ? v.toFixed(1) : Math.round(v).toLocaleString("vi-VN")) + suffix;
      if (p < 1) requestAnimationFrame(tick);
    };
    requestAnimationFrame(tick);
    countIO.unobserve(el);
  }), { threshold: 0.6 });
  $$("[data-count]").forEach(el => countIO.observe(el));

  /* ---------- Header: thu gọn & ẩn khi cuộn xuống ---------- */
  const header = $(".site-header"), fabTop = $(".fab-top");
  let lastY = scrollY, ticking = false;
  const parallax = $$("[data-parallax]");
  const onScroll = () => {
    const y = scrollY;
    header?.classList.toggle("scrolled", y > 10);
    header?.classList.toggle("hide", y > 400 && y > lastY && !document.body.classList.contains("drawer-open"));
    fabTop?.classList.toggle("show", y > 700);
    lastY = y;
    if (!reduce) parallax.forEach(el => {
      const r = el.getBoundingClientRect(), speed = parseFloat(el.dataset.parallax) || 0.15;
      const offset = (r.top + r.height / 2 - innerHeight / 2) * speed;
      el.style.transform = `translate3d(0, ${offset.toFixed(1)}px, 0)`;
    });
    ticking = false;
  };
  addEventListener("scroll", () => { if (!ticking) { requestAnimationFrame(onScroll); ticking = true; } }, { passive: true });
  onScroll();
  fabTop?.addEventListener("click", () => scrollTo({ top: 0, behavior: "smooth" }));

  /* ---------- Hero slider ---------- */
  const slides = $$(".hero-slide"), dots = $$(".hero-caption button"), texts = $$(".hero-copy");
  if (slides.length > 1) {
    let idx = 0, timer;
    const go = i => {
      idx = (i + slides.length) % slides.length;
      slides.forEach((s, k) => s.classList.toggle("active", k === idx));
      dots.forEach((d, k) => { d.classList.remove("active"); if (k === idx) { void d.offsetWidth; d.classList.add("active"); } });
      texts.forEach((t, k) => t.hidden = k !== idx);
      clearTimeout(timer); timer = setTimeout(() => go(idx + 1), 6000);
    };
    dots.forEach((d, k) => d.addEventListener("click", () => go(k)));
    go(0);
  }

  /* ---------- Cánh hoa rơi ---------- */
  $$(".petals").forEach(box => {
    if (reduce) return;
    for (let i = 0; i < 14; i++) {
      const p = document.createElement("i");
      const s = 10 + Math.random() * 16;
      p.style.cssText = `left:${Math.random() * 100}%;width:${s}px;height:${s}px;animation-duration:${9 + Math.random() * 10}s;animation-delay:${-Math.random() * 18}s;opacity:${0.4 + Math.random() * 0.5}`;
      box.appendChild(p);
    }
  });

  /* ---------- Menu mobile, dropdown, search ---------- */
  const body = document.body;
  $$("[data-drawer-open]").forEach(b => b.addEventListener("click", () => body.classList.add("drawer-open")));
  $$("[data-drawer-close], .drawer-backdrop").forEach(b => b.addEventListener("click", () => body.classList.remove("drawer-open", "filters-open")));
  $$("[data-filters-open]").forEach(b => b.addEventListener("click", () => body.classList.add("filters-open")));

  $$(".dropdown > button").forEach(btn => btn.addEventListener("click", e => {
    e.stopPropagation();
    const d = btn.parentElement, open = d.classList.contains("open");
    $$(".dropdown.open").forEach(x => x.classList.remove("open"));
    d.classList.toggle("open", !open);
  }));
  document.addEventListener("click", () => $$(".dropdown.open").forEach(x => x.classList.remove("open")));

  const overlay = $(".search-overlay"), sInput = $("#searchInput"), sResults = $(".search-results");
  const openSearch = () => { overlay.classList.add("open"); setTimeout(() => sInput.focus(), 200); };
  const closeSearch = () => overlay.classList.remove("open");
  $$("[data-search-open]").forEach(b => b.addEventListener("click", () => { body.classList.remove("drawer-open"); openSearch(); }));
  overlay?.addEventListener("click", e => { if (e.target === overlay) closeSearch(); });
  addEventListener("keydown", e => {
    if (e.key === "Escape") { closeSearch(); body.classList.remove("drawer-open", "filters-open"); }
    if (e.key === "/" && !/input|textarea|select/i.test(document.activeElement.tagName)) { e.preventDefault(); openSearch(); }
  });
  let sTimer;
  sInput?.addEventListener("input", () => {
    clearTimeout(sTimer);
    const q = sInput.value.trim();
    if (!q) { sResults.innerHTML = ""; return; }
    sTimer = setTimeout(async () => {
      const r = await fetch(`/api/tim-kiem?q=${encodeURIComponent(q)}`).then(r => r.json()).catch(() => []);
      sResults.innerHTML = r.length
        ? r.map(p => `<a href="${p.url}"><img src="${p.image}" alt=""><div><div>${p.name}</div><small>${p.price}</small></div></a>`).join("")
        : `<p style="color:var(--muted)">Không tìm thấy sản phẩm phù hợp – thử từ khóa khác nhé.</p>`;
    }, 220);
  });

  /* ---------- Giỏ hàng (AJAX) ---------- */
  const badge = $("#cartCount");
  const setCount = n => {
    if (!badge) return;
    badge.textContent = n; badge.hidden = !n;
    badge.classList.remove("bump"); void badge.offsetWidth; badge.classList.add("bump");
  };
  const flyToCart = img => {
    const cart = $("#cartBtn");
    if (!img || !cart || reduce) return;
    const a = img.getBoundingClientRect(), b = cart.getBoundingClientRect();
    const f = img.cloneNode(); f.className = "fly";
    f.style.left = `${a.left + a.width / 2 - 35}px`; f.style.top = `${a.top + a.height / 2 - 35}px`;
    document.body.appendChild(f);
    requestAnimationFrame(() => {
      f.style.transform = `translate(${b.left - a.left - a.width / 2 + 42}px, ${b.top - a.top - a.height / 2 + 22}px) scale(.2)`;
      f.style.opacity = ".3";
    });
    setTimeout(() => f.remove(), 850);
  };
  $$("form[data-ajax-cart]").forEach(form => form.addEventListener("submit", async e => {
    const submitter = e.submitter;
    if (submitter?.name === "buyNow") return; // "Mua ngay" đi thẳng tới thanh toán
    e.preventDefault();
    const btn = form.querySelector("button[type=submit]");
    btn?.setAttribute("disabled", "");
    try {
      const r = await fetch(form.action, { method: "POST", body: new FormData(form), headers: { "X-Requested-With": "XMLHttpRequest" } }).then(r => r.json());
      if (r.ok) {
        flyToCart(form.closest(".product-card, .pd-grid")?.querySelector("img"));
        setTimeout(() => setCount(r.count), 600);
        toast(r.message, ` · <a href="/gio-hang">Xem giỏ</a>`);
      } else toast(r.message || "Có lỗi xảy ra");
    } catch { form.submit(); }
    btn?.removeAttribute("disabled");
  }));

  // Nút tăng/giảm số lượng
  $$(".qty").forEach(q => {
    const input = $("input", q);
    $$("button", q).forEach(b => b.addEventListener("click", () => {
      const v = Math.max(parseInt(input.min || "1"), Math.min(99, (parseInt(input.value) || 1) + parseInt(b.dataset.step)));
      input.value = v; input.dispatchEvent(new Event("change", { bubbles: true }));
    }));
  });

  // Cập nhật số lượng trong trang giỏ hàng không cần tải lại
  $$(".cart-item").forEach(row => {
    const input = $(".qty input", row);
    input?.addEventListener("change", async () => {
      const fd = new FormData();
      fd.append("productId", row.dataset.id); fd.append("quantity", input.value);
      fd.append("__RequestVerificationToken", token());
      const r = await fetch("/gio-hang/cap-nhat", { method: "POST", body: fd, headers: { "X-Requested-With": "XMLHttpRequest" } }).then(r => r.json());
      $(".line-total", row).textContent = r.line;
      $("#sumSubtotal").textContent = r.subtotal; $("#sumShipping").textContent = r.shipping; $("#sumTotal").textContent = r.total;
      const fs = $("#freeShip");
      if (fs) {
        $("span", fs).innerHTML = r.freeShipPercent >= 100
          ? "🎉 Đơn hàng của bạn được <b>miễn phí giao hàng</b>!"
          : `Mua thêm <b>${r.freeShipLeft}</b> để được <b>miễn phí giao hàng</b> 🚚`;
        $(".bar i", fs).style.width = r.freeShipPercent + "%";
      }
      setCount(r.count);
    });
  });

  /* ---------- Tabs, zoom ảnh, xem trước thiệp ---------- */
  $$(".tabs").forEach(t => {
    $$(".tab-btns button", t).forEach((b, i) => b.addEventListener("click", () => {
      $$(".tab-btns button", t).forEach((x, k) => x.classList.toggle("active", k === i));
      $$(".tab-panel", t).forEach((x, k) => x.classList.toggle("active", k === i));
    }));
  });
  $$(".pd-media").forEach(m => {
    const img = $("img", m);
    m.addEventListener("mousemove", e => {
      const r = m.getBoundingClientRect();
      img.style.transformOrigin = `${((e.clientX - r.left) / r.width) * 100}% ${((e.clientY - r.top) / r.height) * 100}%`;
      img.style.transform = "scale(1.6)";
    });
    m.addEventListener("mouseleave", () => img.style.transform = "");
  });
  const msg = $("#CardMessage"), preview = $("#cardPreview");
  if (msg && preview) {
    const upd = () => preview.textContent = msg.value.trim() || "Lời nhắn của bạn sẽ hiển thị trên thiệp…";
    msg.addEventListener("input", upd); upd();
  }
  $$("[data-suggest]").forEach(b => b.addEventListener("click", () => { msg.value = b.dataset.suggest; msg.dispatchEvent(new Event("input")); }));
  $$("[data-confirm]").forEach(f => f.addEventListener("submit", e => { if (!confirm(f.dataset.confirm)) e.preventDefault(); }));

  // Chặn bấm "Đặt hàng" nhiều lần
  $$("form[data-once]").forEach(f => f.addEventListener("submit", () => {
    const b = f.querySelector("button[type=submit]");
    if (b) setTimeout(() => { b.disabled = true; b.innerHTML = "Đang xử lý…"; }, 0);
  }));
})();
