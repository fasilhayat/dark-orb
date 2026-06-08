#!/usr/bin/env python3
"""Generate combat-distribution SVG diagrams.

Dark theme with neon green/yellow/cyan palette.
"""

import math
import os

DIAGRAMS_DIR = os.path.dirname(os.path.abspath(__file__))

# Dark theme neon palette
BG = "#0d1117"
GRID = "rgba(255,255,255,0.06)"
CURVE = "#39ff14"       # neon green
MEAN = "#ffd700"        # neon gold/yellow
CI_1 = "rgba(57,255,20,0.20)"   # 1s CI - neon green 20%
CI_2 = "rgba(57,255,20,0.12)"   # 2s CI - neon green 12%
CI_3 = "rgba(57,255,20,0.06)"   # 3s CI - neon green 6%
CI_1_CMP = "rgba(57,255,20,0.30)"
CI_2_CMP = "rgba(57,255,20,0.16)"
CI_3_CMP = "rgba(57,255,20,0.08)"
TEXT_TITLE = "rgba(255,255,255,0.92)"
TEXT_SUB = "rgba(255,255,255,0.50)"
TEXT_AXIS = "rgba(255,255,255,0.55)"
TEXT_LABEL = "rgba(255,255,255,0.40)"
TEXT_LEGEND = "rgba(255,255,255,0.70)"
TEXT_SCENARIO = "rgba(255,255,255,0.85)"
TEXT_DIM = "rgba(255,255,255,0.35)"
BORDER = "rgba(255,255,255,0.15)"


def fmt(val):
    return f"{val:.1f}"


def fmt_comment(val):
    return str(val).replace(",", ".")


def esc(s):
    return s.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")


def safe_comment(text):
    cleaned = text.replace("--", "- ")
    return f"<!--\n{cleaned}\n-->"


def new_bell_curve(title, scenario, n, hit_rate, mu, sigma, range_lower, range_upper, out_file):
    w, h = 800, 480
    margin_l, margin_r, margin_t, margin_b = 80, 40, 60, 70
    plot_w = w - margin_l - margin_r
    plot_h = h - margin_t - margin_b
    x0, y0 = margin_l, h - margin_b
    max_pdf = 1.0 / (sigma * math.sqrt(2 * math.pi))

    points = []
    data_rows = []
    for hit in range(range_lower, range_upper + 1):
        z = (hit - mu) / sigma
        pdf = math.exp(-0.5 * z * z) * max_pdf
        px = x0 + (hit - range_lower) * plot_w / (range_upper - range_lower)
        py = y0 - pdf * plot_h / max_pdf
        points.append((px, py, hit, z, pdf))
        data_rows.append(f"{hit},{fmt_comment(round(z, 3))},{fmt_comment(f'{pdf:.4e}')}")

    lines = []
    lines.append('<?xml version="1.0" encoding="UTF-8"?>')

    comment_lines = [
        "  COMBAT OUTCOME DISTRIBUTION - Machine-readable dataset",
        f"  Scenario: {esc(scenario)}",
        f"  N={n}  P(hit)={hit_rate}  mu={mu}  sigma={sigma}",
        "  Formula: pdf(x) = exp(-((x-mu)/sigma)^2/2) / (sigma*sqrt(2*pi))",
        "  Data format: hit,z,pdf",
    ]
    for row in data_rows:
        comment_lines.append(f"  {row}")
    lines.append(safe_comment("\n".join(comment_lines)))

    lines.append('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 480" font-family="Consolas, monospace" font-size="12">')
    lines.append(f"  <title>{esc(title)}</title>")
    lines.append(f"  <desc>{esc(scenario)} -- N={n} P(hit)={hit_rate} mu={mu} sigma={sigma}</desc>")
    lines.append(f'<rect width="800" height="480" fill="{BG}"/>')

    # Grid lines
    lines.append(f'<g stroke="{GRID}" stroke-width="0.5">')
    for g in range(5):
        gy = y0 - g * plot_h / 4
        lines.append(f'  <line x1="{x0}" y1="{gy}" x2="{x0 + plot_w}" y2="{gy}"/>')
    lines.append('</g>')

    # CI shaded areas
    for z_val, color in [(3, CI_3), (2, CI_2), (1, CI_1)]:
        c_l = mu - z_val * sigma
        c_r = mu + z_val * sigma
        c_lx = x0 + (c_l - range_lower) * plot_w / (range_upper - range_lower)
        c_rx = x0 + (c_r - range_lower) * plot_w / (range_upper - range_lower)
        fill_pts = []
        for px, py, hit, _, _ in points:
            if math.floor(c_l) <= hit <= math.ceil(c_r):
                fill_pts.append(f"{fmt(px)},{fmt(py)}")
        if len(fill_pts) > 1:
            d = f"M {fill_pts[0]}"
            for fp in fill_pts[1:]:
                d += f" L {fp}"
            d += f" L {fmt(c_rx)},{fmt(y0)} L {fmt(c_lx)},{fmt(y0)} Z"
            lines.append(f"<path d='{d}' fill='{color}' stroke='none'/>")

    # Bell curve path
    d = f"M {fmt(points[0][0])},{fmt(points[0][1])}"
    for i in range(1, len(points)):
        d += f" L {fmt(points[i][0])},{fmt(points[i][1])}"
    lines.append(f"<path d='{d}' fill='none' stroke='{CURVE}' stroke-width='2.5' stroke-linejoin='round' stroke-linecap='round'/>")

    # Mean vertical line
    mu_px = x0 + (mu - range_lower) * plot_w / (range_upper - range_lower)
    lines.append(f"<line x1='{fmt(mu_px)}' y1='{y0}' x2='{fmt(mu_px)}' y2='{y0 - plot_h}' stroke='{MEAN}' stroke-width='2' stroke-dasharray='6,4'/>")
    lines.append(f"<text x='{fmt(mu_px)}' y='{y0 - plot_h - 8}' text-anchor='middle' fill='{MEAN}' font-size='13' font-weight='bold'>mu={mu}</text>")

    # Sigma markers
    lines.append(f'<g font-size="11" fill="{TEXT_LABEL}" text-anchor="middle">')
    for sm in [-3, -2, -1, 0, 1, 2, 3]:
        val = mu + sm * sigma
        v_px = x0 + (val - range_lower) * plot_w / (range_upper - range_lower)
        label = "mu" if sm == 0 else f"+{sm}s" if sm > 0 else f"{sm}s"
        lines.append(f"  <line x1='{fmt(v_px)}' y1='{y0}' x2='{fmt(v_px)}' y2='{y0 + 6}' stroke='{TEXT_LABEL}' stroke-width='1'/>")
        lines.append(f"  <text x='{fmt(v_px)}' y='{y0 + 20}'>{esc(label)}</text>")
    lines.append('</g>')

    # Hit count labels
    lines.append(f'<g font-size="10" fill="{TEXT_DIM}" text-anchor="middle">')
    for z in [-3, -2, -1, 0, 1, 2, 3]:
        val = round(mu + z * sigma)
        v_px = x0 + (val - range_lower) * plot_w / (range_upper - range_lower)
        lines.append(f"  <text x='{fmt(v_px)}' y='{y0 + 36}'>{val}</text>")
    lines.append('</g>')

    # Axis titles
    lines.append(f"<text x='{x0 + plot_w / 2}' y='{y0 + 52}' text-anchor='middle' fill='{TEXT_AXIS}' font-size='13'>Hit count out of {n} attacks</text>")
    lines.append(f"<text x='16' y='{margin_t + plot_h / 2}' text-anchor='middle' fill='{TEXT_AXIS}' font-size='13' transform='rotate(-90,16,{margin_t + plot_h / 2})'>Probability density</text>")

    # Legend
    ci_legend_y = 20
    lines.append(f'<g font-size="11" fill="{TEXT_LEGEND}">')
    for label, color in [
        ("68% CI (+-1s)", CI_1),
        ("95% CI (+-2s)", CI_2),
        ("99.7% CI (+-3s)", CI_3),
        ("Mean (mu)", MEAN),
    ]:
        lx = w - 180
        lines.append(f"  <rect x='{lx}' y='{ci_legend_y}' width='14' height='14' fill='{color}' stroke='{BORDER}' stroke-width='0.5'/>")
        lines.append(f"  <text x='{lx + 20}' y='{ci_legend_y + 11}' fill='{TEXT_LEGEND}'>{esc(label)}</text>")
        ci_legend_y += 18
    lines.append('</g>')

    # Title
    lines.append(f"<text x='{w / 2}' y='22' text-anchor='middle' fill='{TEXT_TITLE}' font-size='15' font-weight='bold'>{esc(title)}</text>")
    lines.append(f"<text x='{w / 2}' y='40' text-anchor='middle' fill='{TEXT_SUB}' font-size='12'>{esc(scenario)} -- P(hit)={hit_rate}  mu={mu}  sigma={sigma:.1f}  N={n}</text>")

    lines.append('</svg>')

    out_path = os.path.join(DIAGRAMS_DIR, out_file)
    with open(out_path, "w", encoding="utf-8") as f:
        f.write("\n".join(lines) + "\n")
    print(f"  {out_file}")


def new_comparison(out_file):
    w, h = 700, 400
    margin_l, margin_r, row_h, row_gap = 180, 80, 50, 30
    plot_w = w - margin_l - margin_r
    bar_y = 80

    scenarios = [
        {"Name": "Balanced", "AP": 10, "DP": 8, "Rate": "60.25%", "Mu": 1205, "Sigma": 21.9, "Lo": 1139, "Hi": 1271},
        {"Name": "Defensive", "AP": 8, "DP": 14, "Rate": "27.50%", "Mu": 550, "Sigma": 19.97, "Lo": 480, "Hi": 620},
        {"Name": "Attacker", "AP": 12, "DP": 5, "Rate": "76.50%", "Mu": 1530, "Sigma": 17.72, "Lo": 1460, "Hi": 1600},
        {"Name": "High Lvl", "AP": 23, "DP": 10, "Rate": "87.75%", "Mu": 1755, "Sigma": 9.97, "Lo": 1700, "Hi": 1810},
    ]

    lines = []
    lines.append('<?xml version="1.0" encoding="UTF-8"?>')

    comment_lines = [
        "  COMBAT DISTRIBUTION COMPARISON - Machine-readable data",
        "  Fields: scenario,ap,dp,hit_rate,mu,sigma,ci68_l,ci68_r,ci95_l,ci95_r,ci997_l,ci997_r",
    ]
    for s in scenarios:
        lo68 = round(s["Mu"] - s["Sigma"])
        hi68 = round(s["Mu"] + s["Sigma"])
        lo95 = round(s["Mu"] - 2 * s["Sigma"])
        hi95 = round(s["Mu"] + 2 * s["Sigma"])
        comment_lines.append(f"  {s['Name']},{s['AP']},{s['DP']},{s['Rate']},{s['Mu']},{s['Sigma']:.2f},{lo68},{hi68},{lo95},{hi95},{s['Lo']},{s['Hi']}")
    lines.append(safe_comment("\n".join(comment_lines)))

    lines.append(f"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 {w} {h}' font-family='Consolas, monospace' font-size='11'>")
    lines.append("  <title>Combat distribution comparison</title>")
    lines.append(f'<rect width="700" height="400" fill="{BG}"/>')

    # Legend
    lines.append(f'<g font-size="10" fill="{TEXT_LEGEND}">')
    leg_y = 20
    for label, color in [
        ("68% CI (+-1s)", CI_1_CMP),
        ("95% CI (+-2s)", CI_2_CMP),
        ("99.7% CI (+-3s)", CI_3_CMP),
        ("Mean (mu)", MEAN),
    ]:
        lines.append(f"  <rect x='{margin_l + 10}' y='{leg_y}' width='12' height='10' fill='{color}' stroke='{BORDER}' stroke-width='0.5'/>")
        lines.append(f"  <text x='{margin_l + 28}' y='{leg_y + 9}' fill='{TEXT_LEGEND}'>{esc(label)}</text>")
        leg_y += 14
    lines.append('</g>')

    for i, s in enumerate(scenarios):
        y = bar_y + i * (row_h + row_gap)
        mu = s["Mu"]
        sigma = s["Sigma"]
        l68 = mu - sigma
        r68 = mu + sigma
        l95 = mu - 2 * sigma
        r95 = mu + 2 * sigma
        span = s["Hi"] - s["Lo"]

        bar_l = margin_l
        bar_r = margin_l + plot_w
        bar95_l = margin_l + (l95 - s["Lo"]) / span * plot_w
        bar95_r = margin_l + (r95 - s["Lo"]) / span * plot_w
        bar68_l = margin_l + (l68 - s["Lo"]) / span * plot_w
        bar68_r = margin_l + (r68 - s["Lo"]) / span * plot_w
        mu_x = margin_l + (mu - s["Lo"]) / span * plot_w

        lines.append(f"  <rect x='{bar_l}' y='{y}' width='{bar_r - bar_l}' height='12' fill='{CI_3_CMP}' rx='2'/>")
        lines.append(f"  <rect x='{bar95_l}' y='{y}' width='{bar95_r - bar95_l}' height='12' fill='{CI_2_CMP}' rx='2'/>")
        lines.append(f"  <rect x='{bar68_l}' y='{y}' width='{bar68_r - bar68_l}' height='12' fill='{CI_1_CMP}' rx='2'/>")
        lines.append(f"  <line x1='{mu_x}' y1='{y - 4}' x2='{mu_x}' y2='{y + 16}' stroke='{MEAN}' stroke-width='2.5'/>")
        lines.append(f"  <line x1='{bar_l}' y1='{y + 12}' x2='{bar_l}' y2='{y + 17}' stroke='{TEXT_LABEL}' stroke-width='1'/>")
        lines.append(f"  <line x1='{bar_r}' y1='{y + 12}' x2='{bar_r}' y2='{y + 17}' stroke='{TEXT_LABEL}' stroke-width='1'/>")

        lines.append(f"  <text x='{margin_l - 8}' y='{y + 10}' text-anchor='end' fill='{TEXT_SCENARIO}' font-weight='bold'>{esc(s['Name'])}</text>")
        lines.append(f"  <text x='{bar_r}' y='{y - 4}' text-anchor='start' fill='{TEXT_DIM}' font-size='10'>P(hit)={s['Rate']}</text>")
        lines.append(f"  <text x='{bar_r}' y='{y + 6}' text-anchor='start' fill='{TEXT_DIM}' font-size='9'>AP={s['AP']} DP={s['DP']}</text>")
        lines.append(f"  <text x='{mu_x}' y='{y + 26}' text-anchor='middle' fill='{MEAN}' font-size='10' font-weight='bold'>mu={mu}</text>")

    lines.append(f'<g font-size="9" fill="{TEXT_DIM}" text-anchor="middle">')
    lines.append(f"  <text x='{margin_l + plot_w / 2}' y='{bar_y + 4 * (row_h + row_gap) + 15}'>Each bar spans the full 99.7% range; darker = narrower CI</text>")
    lines.append('</g>')

    lines.append('</svg>')

    out_path = os.path.join(DIAGRAMS_DIR, out_file)
    with open(out_path, "w", encoding="utf-8") as f:
        f.write("\n".join(lines) + "\n")
    print(f"  {out_file}")


if __name__ == "__main__":
    print("Generating SVGs...")
    new_bell_curve("Hit Distribution - Balanced Combat", "L2 STR12 SR8 vs AC8", 2000, "60.25%", 1205, 21.9, 1139, 1271, "combat-distribution-bellcurve.svg")
    new_bell_curve("Hit Distribution - Defensive Advantage", "L1 STR10 SR8 vs AC14", 2000, "27.50%", 550, 19.97, 480, 620, "combat-distribution-defensive.svg")
    new_bell_curve("Hit Distribution - Attacker Advantage", "L1 STR14 SR10 vs AC5", 2000, "76.50%", 1530, 17.72, 1460, 1600, "combat-distribution-attacker.svg")
    new_bell_curve("Hit Distribution - High Level Scaling", "L5 STR18 SR17 vs AC10", 2000, "87.75%", 1755, 9.97, 1700, 1810, "combat-distribution-highlevel.svg")
    new_comparison("combat-distribution-comparison.svg")
    print("Done.")
