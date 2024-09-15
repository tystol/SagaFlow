import { vitePreprocess } from '@sveltejs/vite-plugin-svelte'

export default {
  // Consult https://svelte.dev/docs#compile-time-svelte-preprocess
  // for more information about preprocessors
  preprocess: vitePreprocess(),
  //emitCss: true, //This is required so that svelte CSS can be processed by other vite plugins
  compilerOptions: {
    discloseVersion: false,
    css: 'external',
    customElement: false,
    preserveWhitespace: false
  },
}
