import App from './App.svelte'

const rootPath = window.location.pathname

const app = new App({
  target: document.getElementById('app'),
  props: {
    rootPath: rootPath,
  },
});

export default app
