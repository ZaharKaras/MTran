import numpy as np

def fast_mat_mul(Q: np.ndarray, A_inv: np.ndarray, i: int) -> np.ndarray:
    rows, cols = A_inv.shape
    res = np.zeros([rows, cols])

    for j in range(rows):
        for k in range(cols):
            a = Q[j][j] * A_inv[j][k]
            if j == i:
                res[j][k] = a
            else:
                b = Q[j][i] * A_inv[i][k]
                res[j][k] = a + b

    return res

if __name__ == '__main__':
    n = int(input("Square matrix size: "))

    A = np.random.randint(-10, 10, size=(n, n))
    x = np.random.randint(-10, 10, size=(n,))

    i = int(input("Index of the matrix column to replace: "))
    assert (i < n)

    A_dash = A.copy()
    A_dash[:, i] = x
    A_inv = np.linalg.inv(A)

    l = A_inv @ x

    if not l[i]:
        raise Exception("Matrix A_dash is irreversible")

    l_wave = l.copy()
    l_wave[i] = -1

    l_hat = (-1 / l[i]) * l_wave

    Q = np.eye(n)
    Q[:, i] = l_hat

    A_dash_inv = fast_mat_mul(Q, A_inv, i)

    print("Original matrix A: \n", A, "\n")
    print(f"Matrix A with column #{i} replaced with {x} (A_dash): \n", A_dash, "\n")
    print("Inverse matrix A_dash: \n", A_dash_inv, "\n")
