import { UserConfig, defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'
import * as path from "path";

// https://vitejs.dev/config/
const config: UserConfig = {
  resolve: {
    alias: {
      $lib: path.resolve("./src/lib")
    }
  },
  build: {
    lib: {
      entry: "./src/web-components.ts",
      formats: ['es'],
      name: "web-components",
      fileName: "web-components"
    },
    outDir: 'dist-web-components'
  },
  plugins: [
      svelte(
        { 
          compilerOptions: { 
            customElement: true 
          }
        }
      )
  ]
}

export default defineConfig(config);
