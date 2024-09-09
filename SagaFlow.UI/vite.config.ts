import { UserConfig, defineConfig,  } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'
import { viteSingleFile } from 'vite-plugin-singlefile'
import path from 'path';
import fs from 'fs/promises';

const replaceAsync = async (str: string, regex: RegExp, asyncFn: (match: any, ...args: any) => Promise<any>) => {
  const promises: Promise<any>[] = []
  str.replace(regex, (match, ...args) => {
      promises.push(asyncFn(match, ...args))
      return match
  })
  const data = await Promise.all(promises)
  return str.replace(regex, () => data.shift())
}

const htmlMacroPlugin = () => {
  return {
      name: "html-macro-transform",
      async transformIndexHtml(html: string) {
        // Regex to match and read xxx: {{inline(xxx)}}
          return replaceAsync(html, /\{\{inline\((.*?)\)\}\}/g, async function (match, capturedPath) {

            let [srcPath, query] = capturedPath.split('?', 2);

            if (srcPath[0] === '/'){
              srcPath = path.resolve(__dirname, srcPath.substring(1));
            }
            
            let data = await fs.readFile(path.resolve(srcPath), 'utf-8')
            if (query === 'uriencoded'){
              data = encodeURIComponent(data);
            }
            return data ?? '';
          });
      },
  };
};

// https://vitejs.dev/config/
const config: UserConfig = {
  resolve: {
    alias: {
      $lib    : path.resolve("./src/lib"),
      $assets : path.resolve("./src/assets"),
      '@'     : path.resolve('./src'),
    }
  },
  build: {
    assetsInlineLimit: Number.MAX_SAFE_INTEGER // force all assets (fonts, SVGs, etc) inline.
  },
  plugins: [
    htmlMacroPlugin(),
    svelte(),
    viteSingleFile()
  ],
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
