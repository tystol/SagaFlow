import { defineConfig } from 'vite';
import { svelte } from '@sveltejs/vite-plugin-svelte';
import { sveltePreprocess } from 'svelte-preprocess';
import path from 'path';

export default defineConfig(({ mode }) => {
  const isDev = mode === 'development';

  return {
    plugins: [
      svelte({
        preprocess: sveltePreprocess(),
        emitCss: false // keep js and css together for library mode,
      })
    ],
    build: {
      lib: {
        entry: path.resolve(__dirname, 'src/index.ts'),
        name: 'HelloPlugin',
        fileName: (format) => `hello-plugin.js`,
        formats: ['es'], // keep simple for dynamic import()
      },
      rollupOptions: {
        // exclude all of svelte runtime so plugin uses host's runtime:
        external: [/^svelte(\/.+)?$/],
        output: {
          globals: {
            svelte: 'svelte'
          }
        }
      }
    },
    server: {
      port: 5174,
      strictPort: true
    },
    define: {
      __DEV__: JSON.stringify(isDev)
    }
  };
});
