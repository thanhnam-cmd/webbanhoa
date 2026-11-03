"""Sinh bộ ảnh minh hoạ hoa (SVG) cho website Floria.

Chạy:  python tools/gen_flowers.py
Kết quả ghi vào WebBanHoa/wwwroot/img/flowers/
"""
import math
import os
import random

OUT = os.path.join(os.path.dirname(__file__), "..", "WebBanHoa", "wwwroot", "img", "flowers")


def shade(hex_color, f):
    """f < 1: tối hơn, f > 1: sáng hơn."""
    h = hex_color.lstrip("#")
    r, g, b = (int(h[i:i + 2], 16) for i in (0, 2, 4))
    if f < 1:
        r, g, b = (int(c * f) for c in (r, g, b))
    else:
        t = f - 1
        r, g, b = (int(c + (255 - c) * t) for c in (r, g, b))
    return "#%02x%02x%02x" % (max(0, min(255, r)), max(0, min(255, g)), max(0, min(255, b)))


def f(v):
    return f"{v:.1f}"


# ------------------------------------------------------------------ flowers
def rose(x, y, r, c, rnd):
    rot = rnd.uniform(0, 360)
    s = [f'<g transform="rotate({f(rot)} {f(x)} {f(y)})">']
    s.append(f'<circle cx="{f(x)}" cy="{f(y)}" r="{f(r)}" fill="{shade(c, .88)}"/>')
    # cánh ngoài dạng vỏ sò
    for i in range(5):
        a = i * 72
        px, py = x + math.cos(math.radians(a)) * r * .38, y + math.sin(math.radians(a)) * r * .38
        s.append(f'<ellipse cx="{f(px)}" cy="{f(py)}" rx="{f(r * .62)}" ry="{f(r * .5)}" fill="{c}" '
                 f'transform="rotate({a} {f(px)} {f(py)})"/>')
    s.append(f'<circle cx="{f(x)}" cy="{f(y)}" r="{f(r * .55)}" fill="{shade(c, 1.12)}"/>')
    dark = shade(c, .62)
    for k, rr in enumerate((r * .5, r * .34, r * .2)):
        a0 = math.radians(k * 110)
        a1 = a0 + math.radians(230)
        x0, y0 = x + math.cos(a0) * rr, y + math.sin(a0) * rr
        x1, y1 = x + math.cos(a1) * rr, y + math.sin(a1) * rr
        s.append(f'<path d="M{f(x0)} {f(y0)} A{f(rr)} {f(rr)} 0 1 1 {f(x1)} {f(y1)}" fill="none" '
                 f'stroke="{dark}" stroke-width="{f(max(1.6, r * .07))}" stroke-linecap="round" opacity=".75"/>')
    s.append(f'<circle cx="{f(x)}" cy="{f(y)}" r="{f(r * .09)}" fill="{dark}"/>')
    s.append("</g>")
    return "".join(s)


def peony(x, y, r, c, rnd):
    s = []
    rot = rnd.uniform(0, 40)
    for ring, (n, dist, rx, ry, col) in enumerate((
            (10, .55, .5, .36, shade(c, .9)), (8, .32, .42, .3, c), (6, .14, .3, .22, shade(c, 1.15)))):
        for i in range(n):
            a = rot + i * 360 / n + ring * 15
            px, py = x + math.cos(math.radians(a)) * r * dist, y + math.sin(math.radians(a)) * r * dist
            s.append(f'<ellipse cx="{f(px)}" cy="{f(py)}" rx="{f(r * rx)}" ry="{f(r * ry)}" fill="{col}" '
                     f'stroke="{shade(c, .78)}" stroke-width="1" transform="rotate({f(a)} {f(px)} {f(py)})"/>')
    s.append(f'<circle cx="{f(x)}" cy="{f(y)}" r="{f(r * .12)}" fill="#F6D77A"/>')
    return "".join(s)


def daisy(x, y, r, c, rnd, center="#F2B636", n=14, layers=1):
    s = []
    rot = rnd.uniform(0, 30)
    for layer in range(layers):
        col = c if layer == 0 else shade(c, 1.1)
        rr = r * (1 - layer * .25)
        for i in range(n):
            a = rot + i * 360 / n + layer * (180 / n)
            px, py = x + math.cos(math.radians(a)) * rr * .55, y + math.sin(math.radians(a)) * rr * .55
            s.append(f'<ellipse cx="{f(px)}" cy="{f(py)}" rx="{f(rr * .46)}" ry="{f(rr * .13)}" fill="{col}" '
                     f'stroke="{shade(c, .85)}" stroke-width=".8" transform="rotate({f(a)} {f(px)} {f(py)})"/>')
    s.append(f'<circle cx="{f(x)}" cy="{f(y)}" r="{f(r * .28)}" fill="{center}"/>')
    s.append(f'<circle cx="{f(x - r * .07)}" cy="{f(y - r * .07)}" r="{f(r * .1)}" fill="{shade(center, 1.25)}" opacity=".7"/>')
    return "".join(s)


def sunflower(x, y, r, rnd):
    s = []
    rot = rnd.uniform(0, 20)
    for layer, col in ((0, "#E9A21C"), (1, "#F7C531")):
        n = 20
        for i in range(n):
            a = rot + i * 360 / n + layer * 9
            tip = r * (1 if layer == 0 else .88)
            ca, sa = math.cos(math.radians(a)), math.sin(math.radians(a))
            bx, by = x + ca * r * .38, y + sa * r * .38
            tx, ty = x + ca * tip, y + sa * tip
            nx, ny = -sa * r * .13, ca * r * .13
            mx, my = (bx + tx) / 2, (by + ty) / 2
            s.append(f'<path d="M{f(bx)} {f(by)} Q{f(mx + nx)} {f(my + ny)} {f(tx)} {f(ty)} Q{f(mx - nx)} {f(my - ny)} {f(bx)} {f(by)}Z" fill="{col}"/>')
    s.append(f'<circle cx="{f(x)}" cy="{f(y)}" r="{f(r * .42)}" fill="#5B3A1E"/>')
    for i in range(26):
        a = i * 137.5
        d = r * .38 * math.sqrt((i + .5) / 26)
        s.append(f'<circle cx="{f(x + math.cos(math.radians(a)) * d)}" cy="{f(y + math.sin(math.radians(a)) * d)}" r="{f(r * .035)}" fill="#8A5A2B"/>')
    return "".join(s)


def tulip(x, y, r, c, rnd):
    lean = rnd.uniform(-12, 12)
    s = [f'<g transform="rotate({f(lean)} {f(x)} {f(y + r)})">']
    s.append(f'<path d="M{f(x)} {f(y + r * .7)} L{f(x)} {f(y + r * 2.6)}" stroke="#5E8B57" stroke-width="{f(r * .14)}" stroke-linecap="round"/>')
    s.append(f'<path d="M{f(x - r * .62)} {f(y - r * .35)} Q{f(x - r * .78)} {f(y + r * .85)} {f(x)} {f(y + r * .85)} '
             f'Q{f(x + r * .78)} {f(y + r * .85)} {f(x + r * .62)} {f(y - r * .35)} L{f(x + r * .3)} {f(y)} '
             f'L{f(x)} {f(y - r * .62)} L{f(x - r * .3)} {f(y)}Z" fill="{c}"/>')
    s.append(f'<path d="M{f(x - r * .3)} {f(y + r * .05)} Q{f(x)} {f(y - r * .9)} {f(x + r * .3)} {f(y + r * .05)} '
             f'Q{f(x + r * .35)} {f(y + r * .7)} {f(x)} {f(y + r * .8)} Q{f(x - r * .35)} {f(y + r * .7)} {f(x - r * .3)} {f(y + r * .05)}Z" fill="{shade(c, 1.18)}"/>')
    s.append(f'<path d="M{f(x - r * .45)} {f(y + r * .2)} Q{f(x - r * .3)} {f(y + r * .7)} {f(x)} {f(y + r * .78)}" fill="none" stroke="{shade(c, .75)}" stroke-width="1.5" opacity=".6"/>')
    s.append("</g>")
    return "".join(s)


def carnation(x, y, r, c, rnd):
    s = []
    for ring, (n, dist, rr, col) in enumerate(((16, .68, .32, shade(c, .9)), (11, .4, .3, c), (6, .14, .24, shade(c, 1.15)))):
        for i in range(n):
            a = i * 360 / n + ring * 11 + rnd.uniform(-4, 4)
            s.append(f'<circle cx="{f(x + math.cos(math.radians(a)) * r * dist)}" cy="{f(y + math.sin(math.radians(a)) * r * dist)}" r="{f(r * rr)}" fill="{col}" stroke="{shade(c, .8)}" stroke-width=".8"/>')
    return "".join(s)


def babys_breath(x, y, spread, rnd, color="#FFFFFF", count=18):
    s = []
    for _ in range(count):
        a = rnd.uniform(0, math.tau)
        d = rnd.uniform(0, spread)
        px, py = x + math.cos(a) * d, y + math.sin(a) * d
        s.append(f'<path d="M{f(x)} {f(y + spread * .6)} Q{f((x + px) / 2 + 6)} {f((y + py) / 2)} {f(px)} {f(py)}" stroke="#7FA37A" stroke-width="1" fill="none" opacity=".7"/>')
        s.append(f'<circle cx="{f(px)}" cy="{f(py)}" r="{f(rnd.uniform(3, 5.5))}" fill="{color}" stroke="#E6E1DA" stroke-width=".6"/>')
    return "".join(s)


def lavender(x, y, h, rnd):
    lean = rnd.uniform(-14, 14)
    s = [f'<g transform="rotate({f(lean)} {f(x)} {f(y + h)})">',
         f'<path d="M{f(x)} {f(y)} L{f(x)} {f(y + h * 1.6)}" stroke="#6E8F63" stroke-width="3"/>']
    for i in range(9):
        yy = y + i * h / 9
        w = 6 + (9 - i) * .35
        for side in (-1, 1):
            s.append(f'<ellipse cx="{f(x + side * w * .5)}" cy="{f(yy)}" rx="{f(w * .55)}" ry="{f(w * .85)}" fill="{"#8E6CC4" if side < 0 else "#A88AD8"}"/>')
    s.append("</g>")
    return "".join(s)


def leaf(x, y, length, width, angle, color):
    d = f"M0 0 Q{f(width)} {f(-length * .5)} 0 {f(-length)} Q{f(-width)} {f(-length * .5)} 0 0Z"
    return (f'<g transform="translate({f(x)} {f(y)}) rotate({f(angle)})"><path d="{d}" fill="{color}"/>'
            f'<path d="M0 0 L0 {f(-length * .92)}" stroke="{shade(color, .75)}" stroke-width="1.5" opacity=".6"/></g>')


def draw_flower(kind, x, y, r, color, rnd):
    if kind == "rose":
        return rose(x, y, r, color, rnd)
    if kind == "peony":
        return peony(x, y, r, color, rnd)
    if kind == "daisy":
        return daisy(x, y, r, color, rnd)
    if kind == "gerbera":
        return daisy(x, y, r, color, rnd, center="#4A2C1A", n=18, layers=2)
    if kind == "mum":
        return daisy(x, y, r, color, rnd, center="#E9D98B", n=22, layers=3)
    if kind == "sunflower":
        return sunflower(x, y, r, rnd)
    if kind == "tulip":
        return tulip(x, y, r, color, rnd)
    if kind == "carnation":
        return carnation(x, y, r, color, rnd)
    return ""


def dome_positions(cx, cy, rx, ry, n, rnd, min_y=None):
    pts = [(cx, cy)]
    rings = [(6, .5), (9, .95)]
    for count, k in rings:
        off = rnd.uniform(0, 60)
        for i in range(count):
            a = math.radians(off + i * 360 / count)
            px, py = cx + math.cos(a) * rx * k, cy + math.sin(a) * ry * k
            if min_y is not None and py > min_y:
                continue
            pts.append((px + rnd.uniform(-8, 8), py + rnd.uniform(-8, 8)))
    return pts[:n]


def flower_cluster(cx, cy, rx, ry, mix, rnd, size=(46, 62), min_y=None, n=16, leaves=True, leaf_color="#6F9A66"):
    out = []
    if leaves:
        for i in range(11):
            a = -170 + i * 34 + rnd.uniform(-8, 8)
            lx = cx + math.cos(math.radians(a - 90)) * rx * .55
            ly = cy + math.sin(math.radians(a - 90)) * ry * .55
            out.append(leaf(lx, ly + 30, rnd.uniform(90, 130), rnd.uniform(22, 32), a, shade(leaf_color, rnd.uniform(.85, 1.1))))
    pts = dome_positions(cx, cy, rx, ry, n, rnd, min_y)
    pts.sort(key=lambda p: p[1])
    fillers = []
    for i, (px, py) in enumerate(pts):
        kind, color = mix[i % len(mix)]
        if kind == "baby":
            fillers.append((px, py))
            continue
        if kind == "lavender":
            out.append(lavender(px, py - 40, 80, rnd))
            continue
        out.append(draw_flower(kind, px, py, rnd.uniform(*size), color, rnd))
    for px, py in fillers:
        out.append(babys_breath(px, py, 34, rnd))
    return "".join(out)


# ------------------------------------------------------------------ arrangements
def bouquet(spec, rnd):
    p1, p2 = spec["paper"]
    s = []
    s.append(f'<path d="M95 210 L505 210 L318 585 L282 585Z" fill="{shade(p1, .88)}"/>')
    s.append(flower_cluster(300, 235, 165, 120, spec["mix"], rnd, min_y=330))
    s.append(f'<path d="M130 330 Q300 380 330 372 L306 588 L294 588Z" fill="{p1}"/>')
    s.append(f'<path d="M470 330 Q300 380 270 372 L294 588 L306 588Z" fill="{p2}"/>')
    s.append(f'<path d="M130 330 Q300 380 330 372" fill="none" stroke="{shade(p1, .75)}" stroke-width="2"/>')
    s.append(ribbon(300, 470, spec.get("ribbon", "#C8506A")))
    return "".join(s)


def ribbon(x, y, c):
    return (f'<path d="M{x} {y} Q{x - 70} {y - 50} {x - 62} {y + 8} Q{x - 40} {y + 20} {x} {y}Z" fill="{c}"/>'
            f'<path d="M{x} {y} Q{x + 70} {y - 50} {x + 62} {y + 8} Q{x + 40} {y + 20} {x} {y}Z" fill="{shade(c, .85)}"/>'
            f'<path d="M{x - 6} {y + 4} Q{x - 30} {y + 50} {x - 40} {y + 90}" stroke="{c}" stroke-width="10" fill="none" stroke-linecap="round"/>'
            f'<path d="M{x + 6} {y + 4} Q{x + 26} {y + 55} {x + 44} {y + 86}" stroke="{shade(c, .85)}" stroke-width="10" fill="none" stroke-linecap="round"/>'
            f'<circle cx="{x}" cy="{y}" r="13" fill="{shade(c, .7)}"/>')


def box(spec, rnd):
    c = spec["box"]
    s = [f'<ellipse cx="300" cy="545" rx="150" ry="26" fill="#000" opacity=".08"/>',
         f'<path d="M160 350 L160 520 A140 34 0 0 0 440 520 L440 350Z" fill="{c}"/>',
         f'<path d="M160 350 L160 520 A140 34 0 0 0 300 554 L300 384 A140 34 0 0 1 160 350Z" fill="{shade(c, .9)}"/>',
         f'<rect x="160" y="430" width="280" height="22" fill="{spec.get("band", "#E9D6B8")}" opacity=".9"/>',
         f'<ellipse cx="300" cy="350" rx="140" ry="34" fill="{shade(c, .7)}"/>']
    s.append(flower_cluster(300, 300, 150, 80, spec["mix"], rnd, size=(40, 52), n=14, leaves=False))
    s.append(f'<text x="300" y="500" text-anchor="middle" font-family="Georgia, serif" font-style="italic" font-size="26" fill="{shade(c, 1.5)}" opacity=".9">Floria</text>')
    return "".join(s)


def basket(spec, rnd):
    c = spec.get("basket", "#C89A62")
    s = [f'<path d="M165 360 Q300 40 435 360" fill="none" stroke="{shade(c, .8)}" stroke-width="16"/>',
         f'<path d="M165 360 Q300 40 435 360" fill="none" stroke="{shade(c, 1.15)}" stroke-width="5" stroke-dasharray="14 10"/>']
    s.append(flower_cluster(300, 300, 160, 90, spec["mix"], rnd, size=(40, 54), n=15, min_y=360))
    s.append(f'<ellipse cx="300" cy="555" rx="140" ry="20" fill="#000" opacity=".08"/>')
    s.append(f'<path d="M150 360 L450 360 L410 545 L190 545Z" fill="{c}"/>')
    for i in range(7):
        y = 375 + i * 25
        s.append(f'<path d="M{f(152 + i * 6)} {y} L{f(448 - i * 6)} {y}" stroke="{shade(c, .8)}" stroke-width="3"/>')
    for i in range(12):
        x = 170 + i * 24
        s.append(f'<path d="M{x} 362 L{f(195 + i * 19)} 543" stroke="{shade(c, 1.15)}" stroke-width="2" opacity=".7"/>')
    s.append(f'<rect x="140" y="350" width="320" height="18" rx="9" fill="{shade(c, .85)}"/>')
    s.append(ribbon(300, 362, spec.get("ribbon", "#C8506A")))
    return "".join(s)


def stand(spec, rnd):
    s = [f'<path d="M260 420 L205 590 M340 420 L395 590 M300 420 L300 590" stroke="#8A6A4A" stroke-width="9" stroke-linecap="round"/>',
         f'<path d="M225 530 L375 530" stroke="#8A6A4A" stroke-width="6"/>']
    s.append(f'<circle cx="300" cy="215" r="160" fill="{spec.get("ring", "#7DA06F")}"/>')
    s.append(flower_cluster(300, 215, 150, 150, spec["mix"], rnd, size=(36, 46), n=16, leaves=False))
    s.append(f'<circle cx="300" cy="430" r="72" fill="{spec.get("ring", "#7DA06F")}"/>')
    s.append(flower_cluster(300, 430, 60, 60, spec["mix"][::-1], rnd, size=(26, 32), n=7, leaves=False))
    rc = spec.get("ribbon", "#C8243B")
    s.append(f'<path d="M150 150 Q300 260 450 150 L455 182 Q300 292 145 182Z" fill="{rc}" opacity=".95"/>')
    s.append(f'<path d="M180 330 L150 430 L172 420 L180 445Z M420 330 L450 430 L428 420 L420 445Z" fill="{shade(rc, .8)}"/>')
    return "".join(s)


def wedding(spec, rnd):
    s = [f'<path d="M270 380 L285 560 L315 560 L330 380Z" fill="#7FA37A"/>']
    for i in range(6):
        s.append(f'<path d="M{276 + i * 9} 380 L{287 + i * 5} 560" stroke="#5E8B57" stroke-width="2"/>')
    s.append(flower_cluster(300, 245, 165, 140, spec["mix"], rnd, size=(42, 56), n=16, leaf_color="#8FB487"))
    rc = spec.get("ribbon", "#F1E3D3")
    s.append(f'<rect x="268" y="420" width="64" height="70" rx="8" fill="{rc}"/>')
    s.append(f'<path d="M290 488 Q260 540 240 590" stroke="{rc}" stroke-width="12" fill="none" stroke-linecap="round"/>')
    s.append(f'<path d="M310 488 Q345 530 350 595" stroke="{shade(rc, .92)}" stroke-width="12" fill="none" stroke-linecap="round"/>')
    s.append(f'<circle cx="300" cy="455" r="6" fill="#D9B98C"/>')
    return "".join(s)


def orchid(spec, rnd):
    c = spec["orchid"]
    s = [f'<ellipse cx="300" cy="568" rx="120" ry="16" fill="#000" opacity=".08"/>']
    # Cành lan vươn lên rồi rủ xuống hai bên
    for sx, cx_, cy_, ex, ey in ((292, 250, 40, 110, 230), (308, 350, 40, 490, 230), (300, 300, 60, 300, 120)):
        s.append(f'<path d="M{sx} 440 Q{cx_} {cy_} {ex} {ey}" stroke="#5E7D4F" stroke-width="5" fill="none"/>')
        steps = 5 if ex != 300 else 2
        for k in range(steps):
            t = .45 + k * (.55 / max(1, steps - 1)) if steps > 1 else .9
            # điểm trên đường cong bậc 2
            px = (1 - t) ** 2 * sx + 2 * (1 - t) * t * cx_ + t * t * ex
            py = (1 - t) ** 2 * 440 + 2 * (1 - t) * t * cy_ + t * t * ey
            r = 50 - k * 4
            s.append(orchid_flower(px, py + 14, r, c, rnd))
    s.append(leaf(250, 445, 120, 40, -70, "#4F7A43") + leaf(350, 445, 120, 40, 70, "#5C8A4F") + leaf(300, 448, 90, 34, 15, "#6A9A5C"))
    pot = spec.get("pot", "#F3EEE7")
    s.append(f'<path d="M205 440 L395 440 L370 565 L230 565Z" fill="{pot}"/>')
    s.append(f'<path d="M300 440 L395 440 L370 565 L300 565Z" fill="{shade(pot, .93)}"/>')
    s.append(f'<rect x="195" y="430" width="210" height="22" rx="6" fill="{shade(pot, .88)}"/>')
    s.append(f'<path d="M225 500 Q300 520 375 500" stroke="#C9A46A" stroke-width="4" fill="none"/>')
    return "".join(s)


def orchid_flower(x, y, r, c, rnd):
    s = []
    for a in (-60, 60):
        px, py = x + math.cos(math.radians(a - 90)) * r * .45, y + math.sin(math.radians(a - 90)) * r * .45
        s.append(f'<ellipse cx="{f(px)}" cy="{f(py)}" rx="{f(r * .38)}" ry="{f(r * .55)}" fill="{shade(c, .95)}" transform="rotate({a} {f(px)} {f(py)})"/>')
    for a in (-110, 110):
        px, py = x + math.cos(math.radians(a - 90)) * r * .5, y + math.sin(math.radians(a - 90)) * r * .5
        s.append(f'<ellipse cx="{f(px)}" cy="{f(py)}" rx="{f(r * .55)}" ry="{f(r * .48)}" fill="{c}" stroke="{shade(c, .85)}" stroke-width="1" transform="rotate({a} {f(px)} {f(py)})"/>')
    s.append(f'<ellipse cx="{f(x)}" cy="{f(y + r * .35)}" rx="{f(r * .22)}" ry="{f(r * .3)}" fill="#C8506A"/>')
    s.append(f'<circle cx="{f(x)}" cy="{f(y)}" r="{f(r * .14)}" fill="#F2C94C"/>')
    return "".join(s)


STYLES = {"bouquet": bouquet, "box": box, "basket": basket, "stand": stand, "wedding": wedding, "orchid": orchid}

PRODUCTS = [
    dict(style="bouquet", bg="#F7E3E1", paper=("#2F2A2C", "#3D3739"), ribbon="#C8243B",
         mix=[("rose", "#C8243B"), ("rose", "#B01E33"), ("baby", ""), ("rose", "#D7334B")]),
    dict(style="bouquet", bg="#FBEDE4", paper=("#FFFFFF", "#F6E1E4"), ribbon="#E7A6B0",
         mix=[("rose", "#F4D3BF"), ("rose", "#F2B8C0"), ("daisy", "#FFFFFF"), ("rose", "#F7E3D2"), ("baby", "")]),
    dict(style="bouquet", bg="#EFE6F6", paper=("#D9C8EC", "#E9DDF4"), ribbon="#8E6CC4",
         mix=[("tulip", "#E87FA4"), ("tulip", "#A77BCA"), ("tulip", "#F2A5C0"), ("tulip", "#C39BE0")]),
    dict(style="bouquet", bg="#FBF0D9", paper=("#C89A62", "#D9AE77"), ribbon="#5E8B57",
         mix=[("sunflower", ""), ("baby", ""), ("sunflower", ""), ("daisy", "#FFFFFF")]),
    dict(style="box", bg="#F9E4E8", box="#E59AA8", band="#F7D8DE",
         mix=[("rose", "#F3A9B8"), ("carnation", "#F7C5CF"), ("rose", "#EE8FA3")]),
    dict(style="box", bg="#FCEBDD", box="#B98B5E", band="#EBD3B5",
         mix=[("rose", "#F08A5D"), ("carnation", "#F7A584"), ("rose", "#F5B27B")]),
    dict(style="box", bg="#F6EEE8", box="#F2ECE6", band="#D8C3A5",
         mix=[("peony", "#F4A6B7"), ("rose", "#F8D7DD"), ("peony", "#EC8FA6")]),
    dict(style="box", bg="#ECE8F3", box="#A07E5C", band="#D9C8EC",
         mix=[("rose", "#B497D6"), ("lavender", ""), ("rose", "#CDB6E6"), ("lavender", "")]),
    dict(style="basket", bg="#FBEFE5", ribbon="#F08A5D",
         mix=[("rose", "#F3A9B8"), ("peony", "#F6C7A8"), ("daisy", "#FFFFFF"), ("rose", "#F7D26B")]),
    dict(style="basket", bg="#F9E7EC", ribbon="#E59AA8",
         mix=[("rose", "#F7C5CF"), ("carnation", "#FFFFFF"), ("rose", "#EE8FA3"), ("baby", "")]),
    dict(style="basket", bg="#FFF1DC", ribbon="#C8506A",
         mix=[("gerbera", "#F26B3A"), ("sunflower", ""), ("gerbera", "#E8394B"), ("gerbera", "#F6A623")]),
    dict(style="stand", bg="#FCE9E3", ring="#7DA06F", ribbon="#C8243B",
         mix=[("gerbera", "#E8394B"), ("rose", "#F7D26B"), ("gerbera", "#F26B3A")]),
    dict(style="stand", bg="#FFF3DA", ring="#6F9A66", ribbon="#D97706",
         mix=[("sunflower", ""), ("gerbera", "#E8394B"), ("sunflower", "")]),
    dict(style="stand", bg="#EEF0F0", ring="#8FA58A", ribbon="#5C6B73",
         mix=[("mum", "#FFFFFF"), ("rose", "#F5F1EA"), ("mum", "#F4F2EC")]),
    dict(style="wedding", bg="#F4EFE8", ribbon="#F1E3D3",
         mix=[("rose", "#FBF8F3"), ("baby", ""), ("rose", "#F3EDE4"), ("daisy", "#FFFFFF")]),
    dict(style="wedding", bg="#F8E9E6", ribbon="#F3D3CC",
         mix=[("peony", "#F4B5BE"), ("rose", "#F7E3D2"), ("peony", "#F8CCD1"), ("baby", "")]),
    dict(style="orchid", bg="#FBF3DA", orchid="#F5CF4E", pot="#FFFFFF"),
    dict(style="orchid", bg="#EEF2EC", orchid="#FFFFFF", pot="#E8E2D6"),
]


def svg(content, bg=None, w=600, h=600):
    back = ""
    if bg:
        back = (f'<rect width="{w}" height="{h}" fill="{bg}"/>'
                f'<circle cx="{w * .5}" cy="{h * .45}" r="{w * .38}" fill="{shade(bg, 1.35)}"/>')
    return (f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {w} {h}" width="{w}" height="{h}">'
            f'{back}{content}</svg>')


def main():
    os.makedirs(OUT, exist_ok=True)
    for i, spec in enumerate(PRODUCTS, 1):
        rnd = random.Random(i * 31)
        body = STYLES[spec["style"]](spec, rnd)
        with open(os.path.join(OUT, f"p{i:02}.svg"), "w", encoding="utf8") as fp:
            fp.write(svg(body, spec["bg"]))

    # Ảnh hero (nền trong suốt, CSS tạo khung vòm phía sau)
    for i, idx in enumerate((0, 2, 11), 1):
        spec = PRODUCTS[idx]
        rnd = random.Random(900 + i)
        body = STYLES[spec["style"]](spec, rnd)
        with open(os.path.join(OUT, f"hero{i}.svg"), "w", encoding="utf8") as fp:
            fp.write(svg(body))

    # Ảnh mặc định
    rnd = random.Random(1)
    with open(os.path.join(OUT, "placeholder.svg"), "w", encoding="utf8") as fp:
        fp.write(svg(rose(300, 300, 120, "#E7A6B0", rnd), "#F6EEE8"))

    # Cánh hoa rơi dùng cho hiệu ứng nền
    with open(os.path.join(OUT, "petal.svg"), "w", encoding="utf8") as fp:
        fp.write('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 40 40"><path d="M20 2 C34 8 38 24 20 38 C2 24 6 8 20 2Z" fill="#F2B8C0"/>'
                 '<path d="M20 6 C22 16 22 26 20 34" stroke="#E59AA8" stroke-width="1.2" fill="none"/></svg>')
    print("Đã tạo ảnh vào", os.path.abspath(OUT))


if __name__ == "__main__":
    main()
