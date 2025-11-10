export function useConfirm() {
  const confirm = (message: string): boolean => {
    return window.confirm(message);
  };

  return { confirm };
}