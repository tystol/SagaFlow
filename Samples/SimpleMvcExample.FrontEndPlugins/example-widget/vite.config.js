import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'
import cssInjectedByJsPlugin from 'vite-plugin-css-injected-by-js'

// https://vitejs.dev/config/
export default defineConfig({
  build: {
    rollupOptions: {
      output: {
        minifyInternalExports: true,
        format: 'modules',
        hoistTransitiveImports: true,
        preserveModules: false,
        entryFileNames: 'example-widget.js', //This is the output file name
        esModule: true
      },
      treeshake: 'smallest',
    },
    lib: {
      formats: ['es'],
      name: 'simple',
      entry: './src/main.js' //This points to an entrypoint like the one defined in the previous code
    },
    target: 'esnext', //Optional, if you don't transpile you save bytes off the final bundle
    emptyOutDir: true,
    outDir: './dist',
    minify: true,
    cssMinify: 'lightningcss',
    assetsInlineLimit: 999999999, //Basically inline every asset
    reportCompressedSize: true
  },
  plugins: [
    svelte(),
    cssInjectedByJsPlugin({ //imported from vite-plugin-css-injected-by-js
      topExecutionPriority: true //Append the css to document.head before writing to document.body
    }),
  ],
})
