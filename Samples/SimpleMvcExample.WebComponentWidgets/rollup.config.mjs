import svelte from "rollup-plugin-svelte";
import commonjs from "@rollup/plugin-commonjs";
import resolve from "@rollup/plugin-node-resolve";
import terser from "@rollup/plugin-terser";
import css from "rollup-plugin-css-only";
import image from '@rollup/plugin-image';
import { globbySync } from "globby";

const production = !process.env.ROLLUP_WATCH;
const formats = ["iife", "umd", "es"];
const components = globbySync("src/components/**/*.svelte").map(
  (path) => {
    let svelteFile = path.split("/")[2];
    return svelteFile.replace(/\.svelte$/, "");
  }
);

export default components.map((component) => ({
  input: `src/components/${component}.svelte`,
  output: formats.map((format) => ({
    name: component,
    file: `public/dist/${format}/${component}.min.js`,
    format,
  })),
  plugins: [
    image(),
    svelte({ compilerOptions: { dev: !production, customElement: true } }),
    css({ output: "bundle.css" }),
    resolve({
      browser: true,
      dedupe: ["svelte"],
    }),
    commonjs(),
    terser(),
  ],
  watch: {
    clearScreen: false,
  },
}));
