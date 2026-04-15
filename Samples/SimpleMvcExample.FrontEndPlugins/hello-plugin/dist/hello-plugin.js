import { SvelteComponent as H, init as S, safe_not_equal as $, noop as x, detach as D, set_data as P, insert as M, append as t, listen as q, element as c, space as w, text as u, attr as C, append_styles as L, component_subscribe as E, run_all as T } from "svelte/internal";
import "svelte/internal/disclose-version";
import { writable as W } from "svelte/store";
function j(n) {
  L(n, "svelte-qwa64b", ".hello-page.svelte-qwa64b{padding:1rem;border:1px solid #ddd;border-radius:8px;background:#f9f9f9}");
}
function y(n) {
  let e, i, a, l, s, o, p, f, m, r, b, h, v, d, g;
  return {
    c() {
      e = c("div"), i = c("h2"), i.textContent = "Hello Page", a = w(), l = c("p"), s = u("Hello, "), o = u(
        /*name*/
        n[0]
      ), p = u("! This is the HelloPage view from hello-plugin."), f = w(), m = c("p"), r = c("button"), b = u("Clicked "), h = u(
        /*count*/
        n[1]
      ), v = u(" times"), C(e, "class", "hello-page svelte-qwa64b");
    },
    m(k, _) {
      M(k, e, _), t(e, i), t(e, a), t(e, l), t(l, s), t(l, o), t(l, p), t(e, f), t(e, m), t(m, r), t(r, b), t(r, h), t(r, v), d || (g = q(
        r,
        "click",
        /*click_handler*/
        n[2]
      ), d = !0);
    },
    p(k, [_]) {
      _ & /*name*/
      1 && P(
        o,
        /*name*/
        k[0]
      ), _ & /*count*/
      2 && P(
        h,
        /*count*/
        k[1]
      );
    },
    i: x,
    o: x,
    d(k) {
      k && D(e), d = !1, g();
    }
  };
}
function z(n, e, i) {
  let { name: a = "World" } = e, l = 0;
  const s = () => i(1, l++, l);
  return n.$$set = (o) => {
    "name" in o && i(0, a = o.name);
  }, [a, l, s];
}
class A extends H {
  constructor(e) {
    super(), S(this, e, z, y, $, { name: 0 }, j);
  }
}
function B(n) {
  L(n, "svelte-x9dfl5", ".settings-panel.svelte-x9dfl5{padding:1rem;border:1px solid #ccc;border-radius:8px;background:#fff}");
}
function F(n) {
  let e, i, a, l, s, o, p, f, m, r = (
    /*$darkMode*/
    n[0] ? "Dark" : "Light"
  ), b, h, v;
  return {
    c() {
      e = c("div"), i = c("h2"), i.textContent = "Settings Panel", a = w(), l = c("label"), s = c("input"), o = u(`\r
    Enable Dark Mode`), p = w(), f = c("p"), m = u("Status: "), b = u(r), C(s, "type", "checkbox"), C(e, "class", "settings-panel svelte-x9dfl5");
    },
    m(d, g) {
      M(d, e, g), t(e, i), t(e, a), t(e, l), t(l, s), s.checked = /*$darkMode*/
      n[0], t(l, o), t(e, p), t(e, f), t(f, m), t(f, b), h || (v = [
        q(
          s,
          "change",
          /*input_change_handler*/
          n[3]
        ),
        q(
          s,
          "change",
          /*toggle*/
          n[2]
        )
      ], h = !0);
    },
    p(d, [g]) {
      g & /*$darkMode*/
      1 && (s.checked = /*$darkMode*/
      d[0]), g & /*$darkMode*/
      1 && r !== (r = /*$darkMode*/
      d[0] ? "Dark" : "Light") && P(b, r);
    },
    i: x,
    o: x,
    d(d) {
      d && D(e), h = !1, T(v);
    }
  };
}
function G(n, e, i) {
  let a;
  const l = W(!1);
  E(n, l, (p) => i(0, a = p));
  function s() {
    l.update((p) => !p);
  }
  function o() {
    a = this.checked, l.set(a);
  }
  return [a, l, s, o];
}
class I extends H {
  constructor(e) {
    super(), S(this, e, G, F, $, {}, B);
  }
}
const O = {
  name: "hello-plugin",
  version: "0.1.0",
  views: {
    hello: A,
    settings: I
  }
};
export {
  O as manifest
};
