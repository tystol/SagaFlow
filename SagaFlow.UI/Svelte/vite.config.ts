import { UserConfig, defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'
import { viteSingleFile } from "vite-plugin-singlefile"
import path from "path";

// https://vitejs.dev/config/
const config: UserConfig = {
  resolve: {
    alias: {
      $lib: path.resolve("./src/lib")
    }
  },
  plugins: [svelte(), viteSingleFile()],
  server: {
    //https: true,
    open: '/sagaflow',
    strictPort: true,
    proxy: {
      '^/sagaflow/.+': { target: 'https://localhost:7172', secure: false }
    }
  }
}

export default defineConfig(config);
